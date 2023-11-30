using System.Collections.Concurrent;
using Garmin.Ble.Lib.Messages;

namespace Garmin.Ble.Lib.Devices.Garmin;

public class FileDownloadQueue
{
    private readonly ILogger _log;
    private readonly ICommunicator _communicator;

    private readonly ConcurrentQueue<QueueItem> _queue = new();

    private QueueItem? _currentItem;
    private readonly object _currentItemLock = new();

    public FileDownloadQueue(ICommunicator communicator, ILogger log)
    {
        _communicator = communicator;
        _log = log;
    }

    public bool InProgress => _currentItem != null || !_queue.IsEmpty;

    public void DownloadFile(int index, string targetFilename)
    {
        _log.Info($"Requesting Download {index}, {targetFilename}");
        _queue.Enqueue(new QueueItem(index, targetFilename));
        CheckStartNextDownload();
    }

    public void ListFiles(int filterType)
    {
        _log.Info("Requesting file list (filter={0})", filterType);
        _queue.Enqueue(new QueueItem(filterType));
        CheckStartNextDownload();
    }

    private const int DirectoryListingFileIndex = 0;

    public void CancelAll()
    {
        _queue.Clear();
        _currentItem = null;
    }

    private void CheckStartNextDownload()
    {
        lock (_currentItemLock)
        {
            if (_currentItem != null)
            {
                _log.Debug("Another download is pending");
                return ;
            }
 
            if (!_queue.Any())
            {
                _log.Debug("No upload in queue");
                return ;
            }
            
            _queue.TryDequeue(out _currentItem);
            if (_currentItem == null)
            {
                _log.DebugInZone(DebugZones.FileTransfer, "TryDequeue returns null");
                return;
            }

        }
        StartNextDownload();
    }

    private void StartNextDownload()
    {
        _log.DebugInZone(DebugZones.FileTransfer, $"Start new Transfer: {_currentItem}");
        if (_currentItem!.FileIndex == DirectoryListingFileIndex)
            _communicator.PostGfdiMessage(new DirectoryFileFilterRequestMessage(_currentItem.FilterType));
        else
            _communicator.PostGfdiMessage(new DownloadRequestMessage(_currentItem.FileIndex, 0, DownloadRequestMessage.REQUEST_NEW_TRANSFER, 0, 0));
    }

    public void OnFileTransferData(FileTransferDataMessage dataMessage)
    {
        if (_currentItem == null || !_currentItem.TransferRequestWasAck)
        {
            _log.Error(_currentItem == null 
                ? "Download data arrived, but nothing is being downloaded"
                : "Download data arrived before Download ACk");
            _communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
                FileTransferDataResponseMessage.RESPONSE_ABORT_DOWNLOAD_REQUEST, 0));
            return;
        }

        if (dataMessage.dataOffset < _currentItem.DataOffset)
        {
            _log.Warn("Ignoring repeated transfer at offset {0} of #{1}", dataMessage.dataOffset,
                _currentItem.FileIndex);
            _communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
                FileTransferDataResponseMessage.RESPONSE_ERROR_DATA_OFFSET_MISMATCH,
                _currentItem.DataOffset));
            return;
        }

        if (dataMessage.dataOffset > _currentItem.DataOffset)
        {
            _log.Warn("Missing data at offset {0} when received data at offset {1} of #{2}",
                _currentItem.DataOffset, dataMessage.dataOffset, _currentItem.FileIndex);
            _communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
                FileTransferDataResponseMessage.RESPONSE_ERROR_DATA_OFFSET_MISMATCH,
                _currentItem.DataOffset));
            return;
        }

        var dataCrc = Crc.Calc(_currentItem.CurrentCrc, dataMessage.data, 0, dataMessage.data.Length);
        if (dataCrc != dataMessage.crc)
        {
            _log.Warn("Invalid CRC ({0} vs {1}) for {2}B data @{3} of {4}", dataCrc, dataMessage.crc,
                dataMessage.data.Length, dataMessage.dataOffset, _currentItem.FileIndex);
            _communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
                    FileTransferDataResponseMessage.RESPONSE_ERROR_CRC_MISMATCH, _currentItem.DataOffset)
                );
            return;
        }

        _currentItem.CurrentCrc = dataCrc;

        _log.Progress();
        _log.DebugInZone(DebugZones.FileTransferProgress, "Received {0}B@{1}/{2} of {3}", dataMessage.data.Length, dataMessage.dataOffset,
            _currentItem.DataSize, _currentItem.FileIndex);
        _currentItem.AppendData(dataMessage.data);

        if (_currentItem.DataOffset >= _currentItem.DataSize)
        {
            var item = _currentItem;
            _log.Info("Transfer of file #{0} complete, {1}/{2}B downloaded", item.FileIndex,
                item.DataOffset, item.DataSize);
            _log.DebugInZone(DebugZones.FileTransfer, $"Complete: {item}");
            ReportCompletedDownload(item);
        }

        _communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
            FileTransferDataResponseMessage.RESPONSE_TRANSFER_SUCCESSFUL, _currentItem.DataOffset));

        if (_currentItem.DataOffset >= _currentItem.DataSize)
        {
            lock(_currentItemLock)
            {
                _currentItem = null;
                CheckStartNextDownload();
            }
        }

    }
    public  void OnDownloadRequestResponse(DownloadRequestResponseMessage responseMessage)
    {
        if (responseMessage.status == GarminConstants.STATUS_ACK && responseMessage.response ==
            DownloadRequestResponseMessage.RESPONSE_DOWNLOAD_REQUEST_OKAY)
        {
            _log.Info("Received response for download request: {0}/{1}, {2}B", responseMessage.status,
                responseMessage.response, responseMessage.fileSize);
            if (_currentItem == null)
            {
               _log.Error($"{nameof(_currentItem)} is null"); 
            }
            else
            {
                _currentItem.TransferRequestWasAck = true;
                _currentItem.DataSize = responseMessage.fileSize;
                _log.DebugInZone(DebugZones.FileTransfer, _currentItem.ToString());
            }
        }
        else
        {
            _log.Error("Received error response for download request : {0}/{1}. Retrying", responseMessage.status,
                responseMessage.response);
            var item = _currentItem;
            _currentItem = null;
            DownloadFile(item!.FileIndex, item.Filename);
        }
    }

    private void ReportCompletedDownload(QueueItem downloadedItem)
    {
        if (downloadedItem == null) throw new ArgumentNullException(nameof(downloadedItem));
        if (downloadedItem.FileIndex == DirectoryListingFileIndex)
        {
            var directoryData = DirectoryData.parse(downloadedItem.Data.ToArray());
            Task.Run(() => OnDirectoryDownloaded(directoryData));
        }
        else
        {
           Task.Run(() => OnFileDownloadComplete(downloadedItem.FileIndex, downloadedItem.Filename, downloadedItem.Data));
        }
    }

    private void OnFileDownloadComplete(int fileIndex, string filename, MemoryStream data)
    {
        if (data.Length == 0)
        {
           _log.Warn($"Download was failed.  Data length is {data.Length} bytes. Retrying...");
           DownloadFile(fileIndex, filename);
        }
        //todo CheckCrc of a fit file
        _log.Info($"Download of a file {fileIndex} is finished. Stored {data.Length} bytes to {filename}.");
        var f = File.OpenWrite($"{filename}");
        data.Position = 0;
        data.CopyTo(f);
        f.Close();
    }

    private void OnDirectoryDownloaded(DirectoryData directoryData)
    {
        void LogFiles(DirectoryEntry[] files)
        {
            _log.Info("-----------------------------------------------------------------------------------------------------");
            foreach (var e in files)
            {
                _log.Info(
                    $"File # {e.fileIndex:000}: type {e.fileDataType}/{e.fileSubType:00} # {e.fileNumber:00000}, {e.fileSize:0000000}B, flags {e.specificFlags}/{e.fileFlags}, timestamp {e.fileDate}");
            }
            _log.Info("-----------------------------------------------------------------------------------------------------");
        }

        string GetFitFilename(DirectoryEntry e)
        {
            return $"{e.fileNumber}_{e.fileDate:dd-MM-yyyy-HH-mm-ss}.fit";
        }

        var directoryEntries = directoryData.entries.OrderBy(d => d.fileNumber).ToArray();
        LogFiles(directoryEntries);

        foreach (var e in directoryEntries)
        {
            if (e is { fileDataType: (int)GarminConstants.FileTypes.FitFile, fileSubType: (int)GarminConstants.FitFileSubTypes.Activity } && 
                !File.Exists(GetFitFilename(e)))
            {
                _log.Info($"{Path.GetFullPath(GetFitFilename(e))}");
                DownloadFile(e.fileIndex, GetFitFilename(e));
            }

            if (e.fileIndex == 0)
            {
                // ?
                _log.Warn("File #0 reported?");
            }
        }
    }


    private class QueueItem
    {
        public bool TransferRequestWasAck { get; set; }
        
        public int FileIndex { get; }
        public readonly MemoryStream Data = new();
        public int DataOffset { get; private set; }

        public int DataSize
        {
            get => Data.Capacity;
            set
            {
                if (value <= 0) return;
                if (Data.Capacity != 0) throw new IllegalStateException("Data size already set");
                Data.Capacity = value;
            }
        }

        public string Filename { get; } = "";

        public int FilterType { get; }

        public QueueItem(int fileIndex, string filename)
        {
            FileIndex = fileIndex;
            DataSize = 0;
            Filename = filename;
            FilterType = 0;
        }
        
        public QueueItem(int filterType)
        {
            FileIndex = DirectoryListingFileIndex;
            DataSize = 0;
            FilterType = filterType;
        }

        public int CurrentCrc { get; set; }


        public void AppendData(ReadOnlySpan<byte> data)
        {
            Data.Write(data);
            DataOffset += data.Length;
        }

        public override string ToString()
        {
            return $"{nameof(Filename)}={Filename}, {nameof(FileIndex)}={FileIndex}, {nameof(FilterType)}={FilterType}, " +
                   $"{nameof(DataSize)}={DataSize}, {nameof(TransferRequestWasAck)}={TransferRequestWasAck} " +
                   $"{nameof(DataOffset)}={DataOffset}, {nameof(Data.Length)}={Data.Length}";
        }
    }
}