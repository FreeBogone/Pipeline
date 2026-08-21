namespace Pipeline.Worker.Discovery;

public sealed class FileStabilityChecker
{
    // Check if a file is stable
    public async Task<bool> IsStableAsync(string filePath, TimeSpan delay, CancellationToken cancellationToken)
    {
        var firstInfo = new FileInfo(filePath);

        if(!firstInfo.Exists)
            return false;

        var firstLength = firstInfo.Length;
        var firstWriteTime = firstInfo.LastWriteTimeUtc;

        await Task.Delay(delay, cancellationToken);

        var secondInfo = new FileInfo(filePath);

        if(!secondInfo.Exists)
            return false;
        
        return firstLength == secondInfo.Length
            && firstWriteTime == secondInfo.LastWriteTimeUtc;
    }
}