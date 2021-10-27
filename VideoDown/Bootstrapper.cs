using Stylet;
using StyletIoC;
using System.Windows;
using System.Windows.Threading;
using VideoDown.ViewModel;

namespace VideoDown
{
    public class Bootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void OnStart()
        {
            /// 这是在应用程序启动之后，但在Ioc容器设置之前调用的。 设置日志等
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Bind your own types. Concrete types are automatically self-bound.
            //builder.Bind<IMyInterface>().To<MyType>();
            //绑定你自己的类型。具体类型是自动自绑定的。
            //建造者。将<IMyInterface>（第页）绑定到<MyType>（第页）
        }

        protected override void Configure()
        {
            // This is called after Stylet has created the IoC container, so this.Container exists,
            // but before the Root ViewModel is launched. Configure your services, etc, in here
            /// 这是在Stylet创建Ioc容器之后调用的。容器存在，但在 启动根视图模型。 在这里配置您的服务等
        }

        protected override void OnLaunch()
        {
            // This is called just after the root ViewModel has been launched Something like a
            // version check that displays a dialog might be launched from here
            ///这是在根viewModel启动后调用的
            /// 类似于显示对话框的版本检查可以从这里启动
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Called on Application.Exit
            //在申请时调用。出口
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            // Called on Application.DispatcherUnhandledException
            //在申请时调用。调度室
        }
    }
}