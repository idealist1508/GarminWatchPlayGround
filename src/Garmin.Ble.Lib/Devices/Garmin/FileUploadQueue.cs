using System.Collections.Concurrent;
using Garmin.Ble.Lib.Messages;

namespace Garmin.Ble.Lib.Devices.Garmin;

public class FileUploadQueue
{
    private readonly ILogger _logger;

    private static readonly int MAX_BLOCK_SIZE = 500;

    // TODO: ?
    private static readonly int UPLOAD_FLAGS = 0;

    private readonly ICommunicator communicator;

    private readonly ConcurrentQueue<QueueItem> _queue = new();

    private QueueItem? currentlyUploadingItem;
    private int currentCrc;
    private long totalRemainingBytes;

    public FileUploadQueue(ICommunicator communicator, ILogger logger)
    {
        this.communicator = communicator;
        _logger = logger;
    }

    public void queueCreateFile(int fileSize, int dataType, int subType, int fileIdentifier, String targetPath,
        Stream data)
    {
        _queue.Enqueue(new QueueItem(fileSize, dataType, subType, fileIdentifier, targetPath, data));
        totalRemainingBytes += fileSize;
        checkStartNextUpload();
    }

    public void queueUploadFile(int fileSize, int fileIndex, Stream data)
    {
        _queue.Enqueue(new QueueItem(fileSize, fileIndex, data));
        totalRemainingBytes += fileSize;
        checkStartNextUpload();
    }

    public void cancelAllUploads()
    {
        _queue.Clear();
        currentlyUploadingItem = null;
    }

    private bool checkStartNextUpload()
    {
        if (currentlyUploadingItem != null)
        {
            _logger.Debug("Another upload is pending");
            return false;
        }

        if (!_queue.Any())
        {
            _logger.Debug("No upload in queue");
            return true;
        }

        startNextUpload();
        return false;
    }

    private void startNextUpload()
    {
        _queue.TryDequeue(out currentlyUploadingItem);
        currentCrc = 0;
        if (currentlyUploadingItem.create)
        {
            _logger.Info("Requesting creation of '{0}' ({1}/{2}/{3}; {4} B)", currentlyUploadingItem.targetPath,
                currentlyUploadingItem.dataType, currentlyUploadingItem.subType, currentlyUploadingItem.fileIdentifier,
                currentlyUploadingItem.fileSize);
            communicator.PostGfdiMessage(new CreateFileRequestMessage(currentlyUploadingItem.fileSize,
                currentlyUploadingItem.dataType, currentlyUploadingItem.subType, currentlyUploadingItem.fileIdentifier,
                0, -1, currentlyUploadingItem.targetPath).Packet);
        }
        else
        {
            _logger.Info("Requesting upload of {0} ({1} B)", currentlyUploadingItem.fileIndex,
                currentlyUploadingItem.fileSize);
            communicator.PostGfdiMessage(new UploadRequestMessage(currentlyUploadingItem.fileIndex, 0,
                DownloadRequestMessage.REQUEST_NEW_TRANSFER, 0).packet);
        }
    }

    public void onCreateFileRequestResponse(CreateFileResponseMessage responseMessage)
    {
        if (currentlyUploadingItem == null)
        {
            _logger.Error("Create file request response arrived, but nothing is being uploaded");
            return;
        }

        if (!currentlyUploadingItem.create)
        {
            _logger.Error("Create file request response arrived, but nothing should have been created");
            return;
        }

        if (responseMessage.status == GarminConstants.STATUS_ACK && responseMessage.response ==
            CreateFileResponseMessage.RESPONSE_FILE_CREATED_SUCCESSFULLY)
        {
            _logger.Info("Received successful response for create file request of '{0}' ({1}/{2}/{3}; {4} B) -> #{5}",
                currentlyUploadingItem.targetPath, currentlyUploadingItem.dataType, currentlyUploadingItem.subType,
                currentlyUploadingItem.fileIdentifier, currentlyUploadingItem.fileSize, responseMessage.fileIndex);
            currentlyUploadingItem.fileIndex = responseMessage.fileIndex;
            communicator.PostGfdiMessage(new UploadRequestMessage(currentlyUploadingItem.fileIndex, 0,
                DownloadRequestMessage.REQUEST_NEW_TRANSFER, 0).packet);
        }
        else
        {
            _logger.Error("Received error response for upload request request of '{0}' ({1}/{2}/{3}; {4} B): {5}, {6}",
                currentlyUploadingItem.targetPath, currentlyUploadingItem.dataType, currentlyUploadingItem.subType,
                currentlyUploadingItem.fileIdentifier, currentlyUploadingItem.fileSize, responseMessage.status,
                responseMessage.response);
            totalRemainingBytes -= currentlyUploadingItem.fileSize;
            currentlyUploadingItem = null;
            checkStartNextUpload();
        }
    }

    public void onUploadRequestResponse(UploadRequestResponseMessage responseMessage)
    {
        if (currentlyUploadingItem == null)
        {
            _logger.Error("Upload request response arrived, but nothing is being uploaded");
            return;
        }

        if (responseMessage.status == GarminConstants.STATUS_ACK &&
            responseMessage.response == UploadRequestResponseMessage.RESPONSE_UPLOAD_REQUEST_OKAY)
        {
            _logger.Info("Received successful response for upload request of {0}: {1}/{2} (max {3}B)",
                currentlyUploadingItem.fileIndex, responseMessage.status, responseMessage.response,
                responseMessage.maxFileSize);
            currentCrc = responseMessage.crcSeed;
        }
        else
        {
            _logger.Error("Received error response for upload request of {0}: {1}/{2}",
                currentlyUploadingItem.fileIndex, responseMessage.status, responseMessage.response);
            totalRemainingBytes -= currentlyUploadingItem.fileSize;
            currentlyUploadingItem = null;
            checkStartNextUpload();
        }
    }

    public void onFileTransferResponse(FileTransferDataResponseMessage dataResponseMessage)
    {
        QueueItem currentlyUploadingItem = this.currentlyUploadingItem;
        if (currentlyUploadingItem == null)
        {
            _logger.Error("Upload request response arrived, but nothing is being uploaded");
            return;
        }

        if (dataResponseMessage.status == GarminConstants.STATUS_ACK && dataResponseMessage.response ==
            FileTransferDataResponseMessage.RESPONSE_TRANSFER_SUCCESSFUL)
        {
            int nextOffset = currentlyUploadingItem.dataOffset + currentlyUploadingItem.blockSize;
            if (dataResponseMessage.nextDataOffset != nextOffset)
            {
                _logger.Warn("Bad expected data offset of #{0}: {1} expected, {2} received",
                    currentlyUploadingItem.fileIndex, currentlyUploadingItem.dataOffset,
                    dataResponseMessage.nextDataOffset);
                communicator.PostGfdiMessage(new FileTransferDataResponseMessage(GarminConstants.STATUS_ACK,
                    FileTransferDataResponseMessage.RESPONSE_ERROR_DATA_OFFSET_MISMATCH,
                    currentlyUploadingItem.dataOffset).Packet);
                return;
            }

            if (nextOffset >= currentlyUploadingItem.fileSize)
            {
                _logger.Info("Transfer of file #{0} complete, {1}/{2}B uploaded", currentlyUploadingItem.fileIndex,
                    nextOffset, currentlyUploadingItem.fileSize);
                this.currentlyUploadingItem = null;
                checkStartNextUpload();
                return;
            }

            // prepare next block
            int blockSize = Math.Min(currentlyUploadingItem.fileSize - nextOffset, MAX_BLOCK_SIZE);
            currentlyUploadingItem.dataOffset = nextOffset;
            currentlyUploadingItem.blockSize = blockSize;
            currentlyUploadingItem.data.Position = nextOffset;
            if (currentlyUploadingItem.reader == null)
                currentlyUploadingItem.reader = new BinaryReader(currentlyUploadingItem.data);
            var blockData = currentlyUploadingItem.reader.ReadBytes(blockSize);
            int blockCrc = Crc.Calc(currentCrc, blockData, 0, blockSize);
            currentlyUploadingItem.blockCrc = blockCrc;

            _logger.Info("Sending {0}B@{1}/{2} of {3}", blockSize, currentlyUploadingItem.dataOffset,
                currentlyUploadingItem.fileSize, currentlyUploadingItem.fileIndex);
            communicator.PostGfdiMessage(new FileTransferDataMessage(UPLOAD_FLAGS, blockCrc,
                currentlyUploadingItem.dataOffset, blockData).packet);
        }
        else
        {
            // TODO: Solve individual responses
            _logger.Error("Received error response for data transfer of {0}: {1}/{2}", currentlyUploadingItem.fileIndex,
                dataResponseMessage.status, dataResponseMessage.response);
            // ??!?
            cancelAllUploads();
        }
    }

    private bool isIdle()
    {
        return currentlyUploadingItem == null;
    }

    private class QueueItem
    {
        public readonly bool create;
        public readonly int fileSize;
        public readonly int dataType;
        public readonly int subType;
        public readonly int fileIdentifier;
        public readonly String targetPath;
        public readonly Stream data;

        public int fileIndex;
        public int dataOffset;
        public int blockSize;
        public int blockCrc;
        public  BinaryReader reader;

        public QueueItem(int fileSize, int dataType, int subType, int fileIdentifier, String targetPath, Stream data)
        {
            this.create = true;
            this.fileSize = fileSize;
            this.dataType = dataType;
            this.subType = subType;
            this.fileIdentifier = fileIdentifier;
            this.targetPath = targetPath;
            this.data = data;
        }


        public QueueItem(int fileSize, int fileIndex, Stream data)
        {
            this.create = false;
            this.fileSize = fileSize;
            this.fileIndex = fileIndex;
            this.data = data;
            this.dataType = 0;
            this.subType = 0;
            this.fileIdentifier = 0;
            this.targetPath = null;
        }
    }
}