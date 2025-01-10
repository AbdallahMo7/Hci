using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Components;

namespace WindowsFormsApp1
{
    internal class ContentLoaderScreen
    {
        private int Width, Height;
        public OptionsPannel optionsPannel;
        private Bitmap background;
        private Rectangle backgroundSrc, backgroundDest;

        public ExibitGalleries exibitGalleries;

        public OptionsPannel VideoImageTextOptions;
        public bool VideoImageTextOptionsActivated = false, OptionsPannelActivated = true, ExibitGalleriesActivated;
        public string currentGallerieTitle = "";
        public bool changed = false;

        public ContentLoaderScreen(int width, int height)
        {
            background = new Bitmap("images/background.jpg");
            optionsPannel = new OptionsPannel(Width, Height);
            optionsPannel.Options.AddRange(new[] { "Ancient Egypt", "Space Exploration", "Modern Art", "Back" });
            optionsPannel.Bitmaps.Add(new Bitmap("images/Exhibits/Ancient Egypt.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/Exhibits/Space Exploration.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/Exhibits/Modern Art.jpg"));
            optionsPannel.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            VideoImageTextOptions = new OptionsPannel(Width, Height);
            VideoImageTextOptions.Options.AddRange(new[] { "Video", "Image", "History", "Back" });
            VideoImageTextOptions.Bitmaps.Add(new Bitmap("images/optionspannel/video.png"));
            VideoImageTextOptions.Bitmaps.Add(new Bitmap("images/optionspannel/image.png"));
            VideoImageTextOptions.Bitmaps.Add(new Bitmap("images/optionspannel/history.jpg"));
            VideoImageTextOptions.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            exibitGalleries = new ExibitGalleries(Width, Height);
            SetDimensions(width, height);
        }
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
            backgroundSrc = new Rectangle(0, 0, background.Width, background.Height);
            backgroundDest = new Rectangle(0, 0, Width, Height);
            if (optionsPannel != null)
                optionsPannel.setDimentions(Width, Height);

            if (exibitGalleries != null)
                exibitGalleries.SetDimensions(Width, Height);

            if (VideoImageTextOptions != null)
                VideoImageTextOptions.setDimentions(Width, Height);

        }
        public void Draw(Graphics g)
        {
            g.DrawImage(background, backgroundDest, backgroundSrc, GraphicsUnit.Pixel);
            g.DrawString("Explore Our Exhibits", new Font("Arial", 12), Brushes.Black, 10, 10);
            if (optionsPannel != null && OptionsPannelActivated)
            {
                optionsPannel.Draw(g);
            }
            if (VideoImageTextOptions != null && VideoImageTextOptionsActivated)
            {
                g.DrawString($"{optionsPannel.Options[optionsPannel.selectedIndex]} Exhibit", new Font("Arial", 12), Brushes.Black, 10, 30);
                VideoImageTextOptions.Draw(g);
            }
            if (exibitGalleries != null && ExibitGalleriesActivated)
            {
                exibitGalleries.Draw(g);
            }
        }
        public void HandleMenuSelection(Point mouse, int radius)
        {
            if (OptionsPannelActivated)
            {
                if(optionsPannel.selectedOption)
                {
                    VideoImageTextOptionsActivated = true;
                    OptionsPannelActivated = false;
                    changed = true;
                    currentGallerieTitle = optionsPannel.Options[optionsPannel.selectedIndex];
                    optionsPannel.selectedOption = false;
                    exibitGalleries.ChooseGallery(optionsPannel.selectedIndex);
                }
            }
            else if (VideoImageTextOptionsActivated)
            {
                VideoImageTextOptions.Hit(mouse, radius);
                if (VideoImageTextOptions.selectedOption)
                {
                    VideoImageTextOptions.selectedOption = false;
                    ExibitGalleriesActivated = exibitGalleries.ChooseMultimedia(VideoImageTextOptions.selectedIndex);
                     
                    if(!ExibitGalleriesActivated)
                    {
                        OptionsPannelActivated = true;
                        exibitGalleries.AncientEgypt = false;
                        exibitGalleries.SpaceExploration = false;
                        exibitGalleries.ModernArt = false;
                    }
                    VideoImageTextOptionsActivated = false;
                    
                }
            }
            else if (ExibitGalleriesActivated)
            {
                ExibitGalleriesActivated = exibitGalleries.HandleMenuSelection(mouse, radius);
                VideoImageTextOptionsActivated = !ExibitGalleriesActivated;
            }
        }
    }

    class ExibitGalleries
    {
        public int Width, Height;
        public bool VideoGallery = false, ImageGallery = false, TextGallery = false;
        public bool AncientEgypt = false, SpaceExploration = false, ModernArt = false;

        public VideoPlayer videoPlayer;
        public ImageViewer imageViewer;
        public TextViewer textViewer;

        public VerticalGallery AncientEgyptVideoGallery;
        string[] AncientEgyptVideoGalleryPathes = { "images/Exhibits/AncientEgypt/Videos/Main.mp4", "images/Exhibits/AncientEgypt/Videos/tutankhamun.mp4" };
        public VerticalGallery SpaceExplorationVideoGallery;
        string[] SpaceExplorationVideoGalleryPathes = { "images/Exhibits/SpaceExploration/Videos/Main.mp4", "images/Exhibits/SpaceExploration/Videos/Apollo 11.mp4" };
        public VerticalGallery ModernArtVideoGallery;
        string[] ModernArtVideoGalleryPathes = { "images/Exhibits/ModernArt/Videos/Main.mp4", "images/Exhibits/ModernArt/Videos/Picasso.mp4" };
        
        public VerticalGallery AncientEgyptImageGallery;
        public VerticalGallery SpaceExplorationImageGallery;
        public VerticalGallery ModernArtImageGallery;
        
        public VerticalGallery AncientEgyptTextGallery;
        string[] AncientEgyptTextGalleryPathes = { "images/Exhibits/AncientEgypt/Text/The pyramids of Giza.txt", "images/Exhibits/AncientEgypt/Text/Ramesses II.txt" };
        public VerticalGallery SpaceExplorationTextGallery;
        string[] SpaceExplorationTextGalleryPathes = { "images/Exhibits/SpaceExploration/Text/Falcon9.txt", "images/Exhibits/SpaceExploration/Text/Mars Rover.txt" };
        public VerticalGallery ModernArtTextGallery;
        string[] ModernArtTextGalleryPathes = { "images/Exhibits/ModernArt/Text/Vincent van Gogh.txt", "images/Exhibits/ModernArt/Text/Modern Art.txt" };


        public ExibitGalleries(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            AncientEgyptVideoGallery = new VerticalGallery(Width, Height);
            AncientEgyptVideoGallery.Options.AddRange(new[] { "Overview", "tutankhamun", "Back" });
            AncientEgyptVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Videos/Main.jpg"));
            AncientEgyptVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Videos/tutankhamun.jpg"));
            AncientEgyptVideoGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));
            SpaceExplorationVideoGallery = new VerticalGallery(Width, Height);
            SpaceExplorationVideoGallery.Options.AddRange(new[] { "Overview", "Apollo 11", "Back" });
            SpaceExplorationVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Videos/Main.jpg"));
            SpaceExplorationVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Videos/Apollo 11.jpg"));
            SpaceExplorationVideoGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            ModernArtVideoGallery = new VerticalGallery(Width, Height);
            ModernArtVideoGallery.Options.AddRange(new[] { "Overview", "Picasso", "Back" });
            ModernArtVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Videos/Main.jpg"));
            ModernArtVideoGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Videos/Picasso.jpg"));
            ModernArtVideoGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            AncientEgyptImageGallery = new VerticalGallery(Width, Height);
            AncientEgyptImageGallery.Options.AddRange(new[] { "Sphinx", "tutankhamun", "Abu Simbel", "Back" });
            AncientEgyptImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Images/Sphinx.jpg"));
            AncientEgyptImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Images/tutankhamun.jpg"));
            AncientEgyptImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Images/Abu Simbel.jpg"));
            AncientEgyptImageGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            SpaceExplorationImageGallery = new VerticalGallery(Width, Height);
            SpaceExplorationImageGallery.Options.AddRange(new[] { "Falkon 9", "Mars Rover", "Back" });
            SpaceExplorationImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Images/Falkon9.jpg"));
            SpaceExplorationImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Images/Mars Rover.jpg"));
            SpaceExplorationImageGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            ModernArtImageGallery = new VerticalGallery(Width, Height);
            ModernArtImageGallery.Options.AddRange(new[] { "Vincent van Gogh Art", "Modern Art", "Vincent van Gogh", "Back" });
            ModernArtImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Images/Vincent van Gogh Art.jpg"));
            ModernArtImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Images/Modern Art.jpg"));
            ModernArtImageGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Images/Vincent van Gogh.jpg"));
            ModernArtImageGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            AncientEgyptTextGallery = new VerticalGallery(Width, Height);
            AncientEgyptTextGallery.Options.AddRange(new[] { "The pyramids of Giza", "Ramesses II", "Back" });
            AncientEgyptTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Text/The pyramids of Giza.jpg"));
            AncientEgyptTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/AncientEgypt/Text/Ramesses II.jpg"));
            AncientEgyptTextGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));
            
            SpaceExplorationTextGallery = new VerticalGallery(Width, Height);
            SpaceExplorationTextGallery.Options.AddRange(new[] { "Falkon 9", "Mars Rover", "Back" });
            SpaceExplorationTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Text/Falkon 9.jpg"));
            SpaceExplorationTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/SpaceExploration/Text/Mars Rover.jpg"));
            SpaceExplorationTextGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));

            ModernArtTextGallery = new VerticalGallery(Width, Height);
            ModernArtTextGallery.Options.AddRange(new[] { "Vincent van Gogh Art", "Modern Art", "Back" });
            ModernArtTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Text/Vincent van Gogh.jpg"));
            ModernArtTextGallery.Bitmaps.Add(new Bitmap("images/Exhibits/ModernArt/Text/Modern Art.jpg"));
            ModernArtTextGallery.Bitmaps.Add(new Bitmap("images/optionspannel/back.png"));
        }
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
            if (AncientEgyptVideoGallery != null)
                AncientEgyptVideoGallery.SetDimensions(Width, Height);
            if (SpaceExplorationVideoGallery != null)
                SpaceExplorationVideoGallery.SetDimensions(Width, Height);
            if (ModernArtVideoGallery != null)
                ModernArtVideoGallery.SetDimensions(Width, Height);
            if (AncientEgyptImageGallery != null)
                AncientEgyptImageGallery.SetDimensions(Width, Height);
            if (SpaceExplorationImageGallery != null)
                SpaceExplorationImageGallery.SetDimensions(Width, Height);
            if (ModernArtImageGallery != null)
                ModernArtImageGallery.SetDimensions(Width, Height);
            if (AncientEgyptTextGallery != null)
                AncientEgyptTextGallery.SetDimensions(Width, Height);
            if (SpaceExplorationTextGallery != null)
                SpaceExplorationTextGallery.SetDimensions(Width, Height);
            if (ModernArtTextGallery != null)
                ModernArtTextGallery.SetDimensions(Width, Height);

            if (videoPlayer != null)
                videoPlayer.SetDimensions(Width, Height);
            if (imageViewer != null)
                imageViewer.SetDimensions(Width, Height);
            if (textViewer != null) 
                textViewer.SetDimensions(Width, Height);
        }
        public void Draw(Graphics g)
        {
            if (AncientEgyptVideoGallery != null && VideoGallery && AncientEgypt)
            {
                AncientEgyptVideoGallery.Draw(g);
            }
            else if (SpaceExplorationVideoGallery != null && VideoGallery && SpaceExploration)
            {
                SpaceExplorationVideoGallery.Draw(g);
            }
            else if (ModernArtVideoGallery != null && VideoGallery && ModernArt)
            {
                ModernArtVideoGallery.Draw(g);
            }
            else if (AncientEgyptImageGallery != null && ImageGallery && AncientEgypt)
            {
                AncientEgyptImageGallery.Draw(g);
            }
            else if (SpaceExplorationImageGallery != null && ImageGallery && SpaceExploration)
            {
                SpaceExplorationImageGallery.Draw(g);
            }
            else if (ModernArtImageGallery != null && ImageGallery && ModernArt)
            {
                ModernArtImageGallery.Draw(g);
            }
            else if (AncientEgyptTextGallery != null && TextGallery && AncientEgypt)
            {
                AncientEgyptTextGallery.Draw(g);
            }
            else if (SpaceExplorationTextGallery != null && TextGallery && SpaceExploration)
            {
                SpaceExplorationTextGallery.Draw(g);
            }
            else if (ModernArtTextGallery != null && TextGallery && ModernArt)
            {
                ModernArtTextGallery.Draw(g);
            }

            if (videoPlayer != null)
            {
                videoPlayer.Draw(g);
            }

            if(ImageGallery && imageViewer != null)
            {
                imageViewer.Draw(g);
            }

            if (TextGallery && textViewer != null) 
            {
                textViewer.Draw(g);
            }
        }
        public void ChooseGallery(int index)
        {
            if (index == 0)
            {
                AncientEgypt = true;
                SpaceExploration = false;
                ModernArt = false;
            }
            else if (index == 1)
            {
                AncientEgypt = false;
                SpaceExploration = true;
                ModernArt = false;
            }
            else if (index == 2)
            {
                AncientEgypt = false;
                SpaceExploration = false;
                ModernArt = true;
            }
            else if (index == 3)
            {
                // Go back to content loader screen (Handled in ContentLoaderScreen)
            }
        }
        public bool ChooseMultimedia(int index)
        {
            if (index == 0)
            {
                VideoGallery = true;
                ImageGallery = false;
                TextGallery = false;
            }
            else if (index == 1)
            {
                VideoGallery = false;
                ImageGallery = true;
                TextGallery = false;
            }
            else if (index == 2)
            {
                VideoGallery = false;
                ImageGallery = false;
                TextGallery = true;
            }
            else if (index == 3)
            {
                VideoGallery = false;
                ImageGallery = false;
                TextGallery = false;
                return false;
            }
            return true;
        }
        public bool HandleMenuSelection(Point mouse, int radius)
        {
            if (!videoPlayer.Showing)
            {
                if (VideoGallery) { return HandleVideoGallerySelection(mouse, radius); }
                if (ImageGallery) { return HandleImageGallerySelection(mouse, radius); }
                if (TextGallery) { return HandleTextGallerySelection(mouse, radius); }
            }

            if (videoPlayer.Showing)
            {
                if (videoPlayer.IsCircleIntersectingRectangle(mouse, radius))
                    videoPlayer.StopVideo();
                return true;
            }
            else
            {
                VideoGallery = false;
                ImageGallery = false;
                TextGallery = false;
                return false;
            }
        }
        private void HandleVideoGalleryHit(Point mouse, int radius)
        {
            if (AncientEgypt)
            {
                AncientEgyptVideoGallery.Hit(mouse, radius);
            }
            else if (SpaceExploration)
            {
                SpaceExplorationVideoGallery.Hit(mouse, radius);
            }
            else if (ModernArt)
            {
                ModernArtVideoGallery.Hit(mouse, radius);
            }
        }
        private void HandleImageGalleryHit(Point mouse, int radius)
        {
            if (AncientEgypt)
            {
                AncientEgyptImageGallery.Hit(mouse, radius);
            }
            else if (SpaceExploration)
            {
                SpaceExplorationImageGallery.Hit(mouse, radius);
            }
            else if (ModernArt)
            {
                ModernArtImageGallery.Hit(mouse, radius);
            }
        }
        private void HandleTextGalleryHit(Point mouse, int radius)
        {
            if (AncientEgypt)
            {
                AncientEgyptTextGallery.Hit(mouse, radius);
            }
            else if (SpaceExploration)
            {
                SpaceExplorationTextGallery.Hit(mouse, radius);
            }
            else if (ModernArt)
            {
                ModernArtTextGallery.Hit(mouse, radius);
            }
        }
        private bool HandleVideoGallerySelection(Point mouse, int radius)
        {
            HandleVideoGalleryHit(mouse, radius);
            if(AncientEgypt)
            {
                if (AncientEgyptVideoGallery.selectedOption)
                {
                    if(AncientEgyptVideoGallery.selectedIndex == AncientEgyptVideoGallery.Options.Count - 1)
                    {
                        VideoGallery = false;
                        return false;
                    }

                    videoPlayer.PlayVideo(AncientEgyptVideoGalleryPathes[AncientEgyptVideoGallery.selectedIndex]);
                    AncientEgyptVideoGallery.selectedOption = false;
                }
            }
            else if (SpaceExploration)
            {
                if (SpaceExplorationVideoGallery.selectedOption)
                {
                    if(SpaceExplorationVideoGallery.selectedIndex == SpaceExplorationVideoGallery.Options.Count - 1)
                    {
                        VideoGallery = false;
                        return false;
                    }
                    videoPlayer.PlayVideo(SpaceExplorationVideoGalleryPathes[SpaceExplorationVideoGallery.selectedIndex]);
                    SpaceExplorationVideoGallery.selectedOption = false;
                }
            }
            else if (ModernArt)
            {
                if (ModernArtVideoGallery.selectedOption)
                {
                    if (ModernArtVideoGallery.selectedIndex == ModernArtVideoGallery.Options.Count - 1)
                    {
                        VideoGallery = false;
                        return false;
                    }

                    videoPlayer.PlayVideo(ModernArtVideoGalleryPathes[ModernArtVideoGallery.selectedIndex]);
                    ModernArtVideoGallery.selectedOption = false;
                }
            }
            return true;
        }
        private bool HandleImageGallerySelection(Point mouse, int radius)
        {
            HandleImageGalleryHit(mouse, radius);
            if (AncientEgypt)
            {
                if (AncientEgyptImageGallery.selectedOption)
                {
                    if (AncientEgyptImageGallery.selectedIndex == AncientEgyptImageGallery.Options.Count - 1)
                    {
                        imageViewer = null;
                        ImageGallery = false;
                        return false;
                    }
                    imageViewer = new ImageViewer(AncientEgyptImageGallery.Bitmaps[AncientEgyptImageGallery.selectedIndex], Width, Height);
                    AncientEgyptImageGallery.selectedOption = false;
                }
            }
            else if (SpaceExploration)
            {
                if (SpaceExplorationImageGallery.selectedOption)
                {
                    if(SpaceExplorationImageGallery.selectedIndex == SpaceExplorationImageGallery.Options.Count - 1)
                    {
                        imageViewer = null;
                        ImageGallery = false;
                        return false;
                    }
                    imageViewer = new ImageViewer(SpaceExplorationImageGallery.Bitmaps[SpaceExplorationImageGallery.selectedIndex], Width, Height);
                    SpaceExplorationImageGallery.selectedOption = false;
                }
            }
            else if (ModernArt)
            {
                if (ModernArtImageGallery.selectedOption)
                {
                    if (ModernArtImageGallery.selectedIndex == ModernArtImageGallery.Options.Count - 1)
                    {
                        imageViewer = null;
                        ImageGallery = false;
                        return false;
                    }
                    imageViewer = new ImageViewer(ModernArtImageGallery.Bitmaps[ModernArtImageGallery.selectedIndex], Width, Height);
                    ModernArtImageGallery.selectedOption = false;
                }
            }
            return true;
        }
        private bool HandleTextGallerySelection(Point mouse, int radius)
        {
            HandleTextGalleryHit(mouse, radius);
            if (AncientEgypt)
            {
                if (AncientEgyptTextGallery.selectedOption)
                {
                    if (AncientEgyptTextGallery.selectedIndex == AncientEgyptTextGallery.Options.Count - 1)
                    {
                        textViewer = null;
                        TextGallery = false;
                        return false;
                    }
                    textViewer = new TextViewer(AncientEgyptTextGallery.Bitmaps[AncientEgyptTextGallery.selectedIndex], AncientEgyptTextGalleryPathes[AncientEgyptTextGallery.selectedIndex], Width, Height);
                    AncientEgyptTextGallery.selectedOption = false;
                }
            }
            else if (SpaceExploration)
            {
                if (SpaceExplorationTextGallery.selectedOption)
                {
                    if (SpaceExplorationTextGallery.selectedIndex == SpaceExplorationTextGallery.Options.Count - 1)
                    {
                        textViewer = null;
                        TextGallery = false;
                        return false;
                    }
                    textViewer = new TextViewer(SpaceExplorationTextGallery.Bitmaps[SpaceExplorationTextGallery.selectedIndex], SpaceExplorationTextGalleryPathes[SpaceExplorationTextGallery.selectedIndex], Width, Height);
                    SpaceExplorationTextGallery.selectedOption = false;
                }
            }
            else if (ModernArt)
            {
                if (ModernArtTextGallery.selectedOption)
                {
                    if (ModernArtTextGallery.selectedIndex == ModernArtTextGallery.Options.Count - 1)
                    {
                        textViewer = null;
                        TextGallery = false;
                        return false;
                    }
                    textViewer = new TextViewer(ModernArtTextGallery.Bitmaps[ModernArtTextGallery.selectedIndex], ModernArtTextGalleryPathes[ModernArtTextGallery.selectedIndex], Width, Height);
                    ModernArtTextGallery.selectedOption = false;
                }
            }
            return true;
        }
    }
}
