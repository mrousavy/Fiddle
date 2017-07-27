using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Fiddle.UI
{
    public static class Extensions
    {
        #region UI
        /// <summary>
        /// Animate a given <see cref="UIElement"/>/<see cref="System.Windows.Controls.Control"/> asynchronous (awaitable)
        /// </summary>
        /// <param name="element">The Element to animate (Button, Label, Window, ..)</param>
        /// <param name="dp">The Property to animate (OpacityProperty, ..)</param>
        /// <param name="from">The beginning value of the animation</param>
        /// <param name="to">The value once the animation finishes</param>
        /// <param name="duration">The duration of this animation</param>
        /// <param name="beginTime">The delay before beginning the animation</param>
        public static async Task AnimateAsync(this UIElement element, DependencyProperty dp, double from, double to, int duration, int beginTime = 0) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(beginTime)
                    };
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        /// <summary>
        /// Animate a given <see cref="UIElement"/>/<see cref="System.Windows.Controls.Control"/> asynchronous (awaitable)
        /// </summary>
        /// <param name="element">The Element to animate (Button, Label, Window, ..)</param>
        /// <param name="dp">The Property to animate (OpacityProperty, ..)</param>
        /// <param name="animation">The animation which will animate the property on the Element</param>
        public static async Task AnimateAsync(this UIElement element, DependencyProperty dp, DoubleAnimation animation) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        /// <summary>
        /// Animate a given <see cref="Animatable"/> asynchronous (awaitable)
        /// </summary>
        /// <param name="element">The Element to animate (Button, Label, Window, ..)</param>
        /// <param name="dp">The Property to animate (OpacityProperty, ..)</param>
        /// <param name="from">The beginning value of the animation</param>
        /// <param name="to">The value once the animation finishes</param>
        /// <param name="duration">The duration of this animation</param>
        /// <param name="beginTime">The delay before beginning the animation</param>
        public static async Task AnimateAsync(this Animatable element, DependencyProperty dp, double from, double to, int duration, int beginTime = 0)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try
            {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(beginTime)
                    };
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            }
            catch
            {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        /// <summary>
        /// Animate a given <see cref="UIElement"/>/<see cref="System.Windows.Controls.Control"/> asynchronous
        /// </summary>
        /// <param name="element">The Element to animate (Button, Label, Window, ..)</param>
        /// <param name="dp">The Property to animate (OpacityProperty, ..)</param>
        /// <param name="from">The beginning value of the animation</param>
        /// <param name="to">The value once the animation finishes</param>
        /// <param name="duration">The duration of this animation</param>
        /// <param name="beginTime">The delay before beginning the animation</param>
        public static async void Animate(this UIElement element, DependencyProperty dp, double from, double to, int duration, int beginTime = 0) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration))
                    {
                        BeginTime = TimeSpan.FromMilliseconds(beginTime)
                    };
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        /// <summary>
        /// Animate a given <see cref="UIElement"/>/<see cref="System.Windows.Controls.Control"/> asynchronous
        /// </summary>
        /// <param name="element">The Element to animate (Button, Label, Window, ..)</param>
        /// <param name="dp">The Property to animate (OpacityProperty, ..)</param>
        /// <param name="animation">The animation which will animate the property on the Element</param>
        public static async void Animate(this UIElement element, DependencyProperty dp, DoubleAnimation animation) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }
        #endregion
    }
}
