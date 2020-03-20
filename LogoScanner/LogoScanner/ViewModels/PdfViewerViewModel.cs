using System.ComponentModel;
using System.IO;

namespace LogoScanner
{
    internal class PdfViewerViewModel : INotifyPropertyChanged
    {
        private Stream m_pdfDocumentStream;

        /// An event to detect the change in the value of a property.
        public event PropertyChangedEventHandler PropertyChanged;

        /// The PDF document stream that is loaded into the instance of the PDF Viewer.
        public Stream PdfDocumentStream
        {
            get => m_pdfDocumentStream;
            set
            {
                m_pdfDocumentStream = value;
                NotifyPropertyChanged(nameof(PdfDocumentStream));
            }
        }

        /// Constructor of the view model class.
        public PdfViewerViewModel()
        {
            //Accessing the PDF document that is added as embedded resource as stream.
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}