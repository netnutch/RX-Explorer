﻿using RX_Explorer.Class;
using System;
using System.IO;
using System.Threading.Tasks;
using TagLib;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace RX_Explorer
{
    public sealed partial class MediaPlayer : Page
    {
        private StorageFile MediaFile;
        public MediaPlayer()
        {
            InitializeComponent();
        }

        private async Task Initialize()
        {
            if (MediaFile.FileType == ".mp3" || MediaFile.FileType == ".flac" || MediaFile.FileType == ".wma" || MediaFile.FileType == ".m4a" || MediaFile.FileType == ".alac")
            {
                MusicCover.Visibility = Visibility.Visible;

                MediaPlaybackItem Item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(MediaFile));

                MediaItemDisplayProperties Props = Item.GetDisplayProperties();
                Props.Type = Windows.Media.MediaPlaybackType.Music;
                Props.MusicProperties.Title = MediaFile.DisplayName;

                try
                {
                    Props.MusicProperties.AlbumArtist = await GetMusicCoverAsync().ConfigureAwait(true);
                }
                catch (Exception)
                {
                    Cover.Visibility = Visibility.Collapsed;
                }
                Item.ApplyDisplayProperties(Props);

                Display.Text = $"{Globalization.GetString("Media_Tip_Text")} {MediaFile.DisplayName}";
                MVControl.Source = Item;
            }
            else
            {
                MusicCover.Visibility = Visibility.Collapsed;

                MediaPlaybackItem Item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(MediaFile));
                MediaItemDisplayProperties Props = Item.GetDisplayProperties();
                Props.Type = Windows.Media.MediaPlaybackType.Video;
                Props.VideoProperties.Title = MediaFile.DisplayName;
                Item.ApplyDisplayProperties(Props);

                MVControl.Source = Item;
            }
        }

        /// <summary>
        /// 异步获取音乐封面
        /// </summary>
        /// <returns>艺术家名称</returns>
        private async Task<string> GetMusicCoverAsync()
        {
            using (Stream FileStream = await MediaFile.OpenStreamForReadAsync().ConfigureAwait(true))
            {
                using (var TagFile = TagLib.File.Create(new StreamFileAbstraction(MediaFile.Name, FileStream, FileStream)))
                {
                    if (TagFile.Tag.Pictures != null && TagFile.Tag.Pictures.Length != 0)
                    {
                        var ImageData = TagFile.Tag.Pictures[0].Data.Data;

                        if (ImageData != null && ImageData.Length != 0)
                        {
                            using (MemoryStream ImageStream = new MemoryStream(ImageData))
                            {
                                BitmapImage bitmap = new BitmapImage
                                {
                                    DecodePixelHeight = 250,
                                    DecodePixelWidth = 250
                                };
                                Cover.Source = bitmap;
                                await bitmap.SetSourceAsync(ImageStream.AsRandomAccessStream());
                            }
                            Cover.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Cover.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        Cover.Visibility = Visibility.Collapsed;
                    }
                    if (TagFile.Tag.AlbumArtists != null && TagFile.Tag.AlbumArtists.Length != 0)
                    {
                        string Artist = "";
                        if (TagFile.Tag.AlbumArtists.Length == 1)
                        {
                            return TagFile.Tag.AlbumArtists[0];
                        }
                        else
                        {
                            Artist = TagFile.Tag.AlbumArtists[0];
                        }
                        foreach (var item in TagFile.Tag.AlbumArtists)
                        {
                            Artist = Artist + "/" + item;
                        }
                        return Artist;
                    }
                    else
                    {
                        return Globalization.GetString("UnknownText");
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MVControl.MediaPlayer.Pause();
            MediaFile = null;
            MVControl.Source = null;
            Cover.Source = null;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            MediaFile = e.Parameter as StorageFile;
            await Initialize().ConfigureAwait(false);
        }

        private void MVControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (MusicCover.Visibility == Visibility.Visible)
            {
                return;
            }
            else
            {
                MVControl.IsFullWindow = !MVControl.IsFullWindow;
            }
        }
    }
}
