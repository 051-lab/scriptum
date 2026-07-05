namespace Scriptum.Services;

public static class PageImagePreprocessingServiceFactory
{
    public static IPageImagePreprocessingService CreateDefault()
    {
        var useOpenCv = string.Equals(
            Environment.GetEnvironmentVariable("SCRIPTUM_USE_OPENCV_PREPROCESSING"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (useOpenCv)
        {
            return new OpenCvPageImagePreprocessingService(new OpenCvPageImagePreprocessingOptions
            {
                Enabled = true
            });
        }

        return new NoOpPageImagePreprocessingService();
    }
}
