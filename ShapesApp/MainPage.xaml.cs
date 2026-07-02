using Newtonsoft.Json;
using Refit;
/* COMMENT out the line to use Approov SDK */
using System.Net.Http;
/* UNCOMMENT the lines bellow to use Approov SDK */
//using Approov;
namespace ShapesApp;

public partial class MainPage : ContentPage
{
    /* The shapes server URL */
    static string baseURL = "https://shapes.approov.io";
    /* The secret key: REPLACE with shapes_api_key_placeholder if using SECRETS-PROTECTION */
    string shapes_api_key = "yXClypapWNHIifHUWmBIyPFAm";
    // Refit API interface
    private IApiInterface apiClient;
    /* COMMENT this line if using Approov */
    private static HttpClient httpClient;
    /* UNCOMMENT this line if using Approov */
    //private static ApproovHttpClient httpClient;
    public MainPage()
    {
        InitializeComponent();
        /* COMMENT out the line to use Approov SDK */
        httpClient = new HttpClient();
        /* UNCOMMENT the lines bellow to use Approov SDK */
        //ApproovService.Initialize("<enter-your-config-string-here>");
        //httpClient = new ApproovHttpClient();
        // Add substitution header: Uncomment if using SECRETS-PROTECTION
        //ApproovService.AddSubstitutionHeader("Api-Key", null);
        httpClient.BaseAddress = new Uri(baseURL);
        httpClient.DefaultRequestHeaders.Add("Api-Key", shapes_api_key);
        try
        {
            apiClient = RestService.For<IApiInterface>(httpClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during RestService: " + ex.Message);
        }
    }

    private async void OnHelloButtonClicked(object sender, EventArgs e)
    {
        
        try
        {
            Dictionary<string, string> response = await apiClient.GetHello().ConfigureAwait(false);
            if (response.ContainsKey("text"))
            {
                // Set status image
                Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = "hello.png");
                // Set status label
                Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = response["text"]);
                
            }
            else
            {
                // Set status image
                Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = "confused.png");
                // Set status label
                Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = "Error getting Hello from Shapes server");
            }
        }
        catch (Exception ex)
        {
            // Set status image
            Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = "confused.png");
            // Set status label
            Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = "Exception getting Hello from Shapes server: " + ex.Message);
        }
    }

    private async void OnShapeButtonClicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            Dictionary<string, string> response = await apiClient.GetShape().ConfigureAwait(false);
            if (response.ContainsKey("shape")) {
                string shapeImageName = response["shape"].ToLower() + ".png";
                // Set status image
                Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = shapeImageName);
                // Set status label
                Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = "200 OK");
            }
                
            else
            {
                // Set status image
                Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = "confused.png");
                // Set status label
                Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = "Error getting Shape: response json malformed");
            }
        }
        catch (Exception ex)
        {
            // Set status image
            Application.Current.MainPage.Dispatcher.Dispatch(() => logoImage.Source = "confused.png");
            // Set status label
            Application.Current.MainPage.Dispatcher.Dispatch(() => textMessage.Text = "Exception getting Shape: " + ex.Message);
        }
    }

}


public interface IApiInterface
{
    [Get("/v1/hello/")]
    Task<Dictionary<string, string>> GetHello();
    [Get("/v1/shapes/")]
    Task<Dictionary<string, string>> GetShape();
}