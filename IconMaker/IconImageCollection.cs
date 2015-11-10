using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace IconMaker
{
    /// <summary>
    /// Maintains a sorted collection of icon images.
    /// </summary>
    public sealed class IconImageCollection : ICollection<BitmapSource>, INotifyCollectionChanged
    {
        /// <summary>
        /// Stores icon images.
        /// </summary>
        private readonly SortedList<int, BitmapSource> images = new SortedList<int, BitmapSource>();

        /// <summary>
        /// Initializes a new instance of the IconImageCollection class.
        /// </summary>
        public IconImageCollection()
        {
        }

        /// <summary>
        /// Occurs when the collection has changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets the number of images in the collection.
        /// </summary>
        public int Count
        {
            get { return this.images.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        bool ICollection<BitmapSource>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Adds a new image to the collection.
        /// </summary>
        /// <param name="item">Image to add.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False positive. Parameters are validated.")]
        public void Add(BitmapSource item)
        {
            var ex = ValidateNewItem(item);
            if(ex != null)
                throw ex;

            if(this.images.ContainsKey(item.PixelWidth))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.SizeAlreadyPresent, item.PixelWidth));

            this.images.Add(item.PixelWidth, ConvertBitmap(item));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds an image to the collection, replacing one if it has the same dimensions.
        /// </summary>
        /// <param name="item">Image to add.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False positive. Parameters are validated.")]
        public void Set(BitmapSource item)
        {
            var ex = ValidateNewItem(item);
            if(ex != null)
                throw ex;

            BitmapSource currentItem;
            if(this.images.TryGetValue(item.PixelWidth, out currentItem))
            {
                if(item == currentItem)
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

        /// <summary>
        /// Removes all of the images from the collection.
        /// </summary>
        public void Clear()
        {
            this.images.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Returns a value indicating whether an image is contained in the collection.
        /// </summary>
        /// <param name="item">Image to search for in the collection.</param>
        /// <returns>Value indicating whether the image was found in the collection.</returns>
        public bool Contains(BitmapSource item)
        {
            return this.images.ContainsValue(item);
        }

        /// <summary>
        /// Copies the images in the collection to an array.
        /// </summary>
        /// <param name="array">Array into which images are copied.</param>
        /// <param name="arrayIndex">Index in array to begin copying.</param>
        public void CopyTo(BitmapSource[] array, int arrayIndex)
        {
            this.images.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes an image from the collection.
        /// </summary>
        /// <param name="item">Image to remove from the collection.</param>
        /// <returns>Value indicating whether image was contained in the collection.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Parameter is not directly used, so it does not need validation.")]
        public bool Remove(BitmapSource item)
        {
            bool res = this.images.ContainsValue(item);
            if(res)
            {
                this.images.Remove(item.PixelWidth);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            return res;
        }

        /// <summary>
        /// Returns an instance used to enumerate the images in the collection.
        /// </summary>
        /// <returns>Instance used to enumerate the images in the collection.</returns>
        public IEnumerator<BitmapSource> GetEnumerator()
        {
            return this.images.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an instance used to enumerate the images in the collection.
        /// </summary>
        /// <returns>Instance used to enumerate the images in the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns an exception to be raised if the item fails validation.
        /// </summary>
        /// <param name="item">Image to validate.</param>
        /// <returns>Exception to be raised if the item fails; otherwise null.</returns>
        private static Exception ValidateNewItem(BitmapSource item)
        {
            if(item == null)
                return new ArgumentNullException("item");
            if(item.PixelWidth != item.PixelHeight)
                return new ArgumentException(Properties.Resources.WidthHeightNotEqual);
            if(item.PixelWidth > 256)
                return new ArgumentException(Properties.Resources.ImageTooBig);
            if(item.PixelWidth < 16)
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
            if(source.Format == PixelFormats.Bgra32)
                return source;

            return new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);
        }

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="e">Contains information about the event.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = this.CollectionChanged;
            if(handler != null)
                handler(this, e);
        }
    }
}
