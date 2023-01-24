using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Media;

namespace IconMaker
{
    /// <summary>
    /// Maintains a sorted collection of icon images.
    /// </summary>
    public sealed class IconImageCollection : ICollection<BitmapSource>, INotifyCollectionChanged
    {
        private readonly SortedList<int, BitmapSource> images = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public int Count => this.images.Count;

        bool ICollection<BitmapSource>.IsReadOnly => false;

        public void Add(BitmapSource item)
        {
            var ex = ValidateNewItem(item);
            if (ex != null)
                throw ex;

            if (this.images.ContainsKey(item.PixelWidth))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.SizeAlreadyPresent, item.PixelWidth));

            this.images.Add(item.PixelWidth, ConvertBitmap(item));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void Set(BitmapSource item)
        {
            var ex = ValidateNewItem(item);
            if (ex != null)
                throw ex;

            if (this.images.TryGetValue(item.PixelWidth, out var currentItem))
            {
                if (item == currentItem)
                    return;

                this.images[item.PixelWidth] = ConvertBitmap(item);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                this.images.Add(item.PixelWidth, ConvertBitmap(item));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public void Clear()
        {
            this.images.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public bool Contains(BitmapSource item)
        {
            return this.images.ContainsValue(item);
        }
        public void CopyTo(BitmapSource[] array, int arrayIndex)
        {
            this.images.Values.CopyTo(array, arrayIndex);
        }
        public bool Remove(BitmapSource item)
        {
            bool res = this.images.ContainsValue(item);
            if (res)
            {
                this.images.Remove(item.PixelWidth);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            return res;
        }
        public IEnumerator<BitmapSource> GetEnumerator()
        {
            return this.images.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static Exception? ValidateNewItem(BitmapSource item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.PixelWidth != item.PixelHeight)
                return new ArgumentException(Properties.Resources.WidthHeightNotEqual);
            if (item.PixelWidth > 256)
                return new ArgumentException(Properties.Resources.ImageTooBig);
            if (item.PixelWidth < 16)
                return new ArgumentException(Properties.Resources.ImageTooSmall);

            return null;
        }
        /// <summary>
        /// Converts a bitmap to BRGA32 pixel format if necessary.
        /// </summary>
        /// <param name="source">Bitmap to convert.</param>
        /// <returns>Converted bitmap.</returns>
        private static BitmapSource ConvertBitmap(BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgra32)
                return source;

            return new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);
        }
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => this.CollectionChanged?.Invoke(this, e);
    }
}
