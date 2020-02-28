using Acr.UserDialogs;

namespace LogoScanner
{
    public class DialogService
    {
        public static void ShowLoading()
        {
            UserDialogs.Instance.ShowLoading();
        }

        public static void ShowLoading(string loadingMessage)
        {
            UserDialogs.Instance.ShowLoading(loadingMessage);
        }

        public static void HideLoading()
        {
            UserDialogs.Instance.HideLoading();
        }

        public static void ShowError(string errorMessage)
        {
            UserDialogs.Instance.Alert(errorMessage, title:"Error");
        }

        public static void ShowSuccess(string successMessage)
        {
            UserDialogs.Instance.Alert(successMessage, title: "Success");
        }

        public static void ShowSuccess(string successMessage, int timeOut)
        {
            //UserDialogs.Instance.AlertAsync(successMessage, title: "Success", cancelToken:timeOut);
        }
    }
}
