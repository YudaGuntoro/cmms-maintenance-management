using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace MaintenanceApp.Handler
{
    public class CustomRefreshViewHandler : RefreshViewHandler
    {
#if ANDROID
        protected override void ConnectHandler(MauiSwipeRefreshLayout platformView)
        {
            int deviceOffset = platformView?.ProgressViewEndOffset ?? 0;
            base.ConnectHandler(platformView);
            platformView?.SetProgressViewEndTarget(true, deviceOffset);

        }
#endif
    }
}
