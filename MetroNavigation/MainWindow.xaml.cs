using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetroNavigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Station start;
        private Station end;
        private Metro metro = new Metro();
        private TextBlock textStart = new TextBlock();
        private TextBlock textEnd = new TextBlock();
        private Rectangle aRectangle = new Rectangle();

        public MainWindow()
        {
            InitializeComponent();

            textStart.Text = "Відправлення з: не вибрана";
            textStart.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textStart, 650);
            Canvas.SetTop(textStart, 30);
            MainGrid.Children.Add(textStart);

            textEnd.Text = "Прибуття в: не вибрана";
            textEnd.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textEnd, 650);
            Canvas.SetTop(textEnd, 50);
            MainGrid.Children.Add(textEnd);

        }

        private void animation(PathFigure pFigure, int animationTime)
        {
            // Create a NameScope for the page so that
            // we can use Storyboards.
            NameScope.SetNameScope(this, new NameScope());

            // Create a rectangle.
            aRectangle.Width = 20;
            aRectangle.Height = 20;
            aRectangle.Fill = new ImageBrush(new BitmapImage(new Uri(@"image/train.png", UriKind.Relative)));

            // Create a transform. This transform
            // will be used to move the rectangle.
            TranslateTransform animatedTranslateTransform =
                new TranslateTransform();

            // Register the transform's name with the page
            // so that they it be targeted by a Storyboard.
            this.RegisterName("AnimatedTranslateTransform", animatedTranslateTransform);

            aRectangle.RenderTransform = animatedTranslateTransform;

            // Create a Canvas to contain the rectangle
            // and add it to the page.
            Canvas mainPanel = MainGrid;
            mainPanel.Children.Add(aRectangle);
            this.Content = mainPanel;

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a DoubleAnimationUsingPath to move the
            // rectangle horizontally along the path by animating 
            // its TranslateTransform.
            DoubleAnimationUsingPath translateXAnimation =
                new DoubleAnimationUsingPath();
            translateXAnimation.PathGeometry = animationPath;
            translateXAnimation.Duration = TimeSpan.FromSeconds(animationTime);

            // Set the Source property to X. This makes
            // the animation generate horizontal offset values from
            // the path information. 
            translateXAnimation.Source = PathAnimationSource.X;

            // Set the animation to target the X property
            // of the TranslateTransform named "AnimatedTranslateTransform".
            Storyboard.SetTargetName(translateXAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateXAnimation,
                new PropertyPath(TranslateTransform.XProperty));

            // Create a DoubleAnimationUsingPath to move the
            // rectangle vertically along the path by animating 
            // its TranslateTransform.
            DoubleAnimationUsingPath translateYAnimation =
                new DoubleAnimationUsingPath();
            translateYAnimation.PathGeometry = animationPath;
            translateYAnimation.Duration = TimeSpan.FromSeconds(animationTime);

            // Set the Source property to Y. This makes
            // the animation generate vertical offset values from
            // the path information. 
            translateYAnimation.Source = PathAnimationSource.Y;

            // Set the animation to target the Y property
            // of the TranslateTransform named "AnimatedTranslateTransform".
            Storyboard.SetTargetName(translateYAnimation, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(translateYAnimation,
                new PropertyPath(TranslateTransform.YProperty));

            // Create a Storyboard to contain and apply the animations.
            Storyboard pathAnimationStoryboard = new Storyboard();
        //    pathAnimationStoryboard.RepeatBehavior = RepeatBehavior.ReferenceEquals;
            pathAnimationStoryboard.Children.Add(translateXAnimation);
            pathAnimationStoryboard.Children.Add(translateYAnimation);

            // Start the animations when the rectangle is loaded.
            aRectangle.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                // Start the storyboard.
                pathAnimationStoryboard.Begin(this);
            };
        }

        private void MouseLeftButtonDownOnGrid(object sender, MouseButtonEventArgs e)
        {
            if (start == null)
            {
                if ((start = metro.findStationInLine(Mouse.GetPosition(MainGrid))) != null)
                {
                    textStart.Text = "Відправлення з " + start.stationName;
                }
            }
            else if (end == null)
            {
                if ((end = metro.findStationInLine(Mouse.GetPosition(MainGrid))) != null)
                {
                    textEnd.Text = "Прибуття в " + end.stationName;

                    PathFigure pFigure = metro.findPath(start, end);
                    if (pFigure == null)
                        MessageBox.Show("Помилка пошуку шляху");
                    else
                        animation(pFigure, pFigure.Segments.Count);
                }
            }
            else if (end != null && start != null)
            {
                textStart.Text = "Відправлення з: не вибрана";
                textEnd.Text = "Прибуття в: не вибрана";
                end = null;
                start = null;
                MainGrid.Children.Remove(aRectangle);
            }
        }

        private void MouseRightButtonDownOnGrid(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(MainGrid);
            MessageBox.Show("Mouse on: x = " + p.X + " y = " + p.Y);
        }
    }
}
