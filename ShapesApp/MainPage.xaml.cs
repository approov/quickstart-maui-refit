using Refit;

// ════════════════════════════════════════════════════════════════════════════
//  Approov Shapes example (Refit)
//
//  This one file drives the whole quickstart. You progress through three stages,
//  each just a small edit here (full walkthrough in SHAPES-EXAMPLE.md):
//
//    Stage 1 · v1  — No Approov.        Plain HttpClient + Refit + a hard-coded API key.
//    Stage 2 · v3  — API protection.    ApproovHttpClient adds a verified Approov token.
//    Stage 3 · v5  — + message signing. Adds an RFC 9421 HTTP message signature.
//
//  Each stage is marked "STAGE n" below. To move up a stage, change
//  ENDPOINT_VERSION and comment/uncomment the STAGE lines as indicated.
//  The Refit interface paths never change — the endpoint version lives in the
//  base URL only (see IApiInterface at the bottom of this file).
// ════════════════════════════════════════════════════════════════════════════

// STAGE 2+ : uncomment to use the Approov service layer.
//using Approov;
// STAGE 3  : uncomment as well to use HTTP message signing.
//using Approov.Util.Sig;

namespace ShapesApp;

public partial class MainPage : ContentPage
{
    // ── Configuration ────────────────────────────────────────────────────────

    // Endpoint version:  "v1" unprotected · "v3" Approov token · "v5" token + signing
    const string ENDPOINT_VERSION = "v1";

    // STAGE 2+ : your Approov config string from the onboarding email
    //            (looks like "#123456#K/XPlLtfcwnWkzv99Wj5VmAxo4CrU267J1KlQyoz8Qo=").
    const string APPROOV_CONFIG = "<enter-your-config-string-here>";

    // The Shapes API key. For SECRETS-PROTECTION, replace with
    // "shapes_api_key_placeholder" and follow SECRETS-PROTECTION.md.
    const string SHAPES_API_KEY = "yXClypapWNHIifHUWmBIyPFAm";

    // The endpoint version lives here, so the Refit interface paths stay fixed.
    // Refit appends each interface path to this base, e.g. ".../v3" + "/shapes/".
    static readonly string BaseUrl = $"https://shapes.approov.io/{ENDPOINT_VERSION}";

    // The Refit-generated client that talks to the Shapes API.
    private IApiInterface apiClient;

    // STAGE 1 : plain HttpClient.  STAGE 2+ : switch the type to ApproovHttpClient
    //           (comment the first line, uncomment the second).
    private static HttpClient httpClient;
    //private static ApproovHttpClient httpClient;

    public MainPage()
    {
        InitializeComponent();

        // ── STAGE 1 · v1 — no Approov ────────────────────────────────────────
        // Runs against the unprotected endpoint using the hard-coded API key.
        httpClient = new HttpClient();

        // ── STAGE 2 · v3 — Approov API protection ────────────────────────────
        //   1. set ENDPOINT_VERSION = "v3"
        //   2. comment out the STAGE 1 `new HttpClient()` line above
        //   3. uncomment `using Approov;` and the two lines below
        //ApproovService.Initialize(APPROOV_CONFIG);
        //httpClient = new ApproovHttpClient();

        // ── STAGE 3 · v5 — add HTTP message signing ──────────────────────────
        // In addition to STAGE 2:
        //   1. set ENDPOINT_VERSION = "v5"
        //   2. uncomment `using Approov.Util.Sig;`
        //   3. uncomment the registration below (install signing is the default)
        //ApproovService.SetServiceMutator(
        //    new ApproovDefaultMessageSigning().SetDefaultFactory(
        //        ApproovDefaultMessageSigning.GenerateDefaultSignatureParametersFactory()));

        // SECRETS-PROTECTION (alternative to STAGE 2+): keep the STAGE 2 Approov
        // lines above in place, set ENDPOINT_VERSION = "v1" and
        // SHAPES_API_KEY = "shapes_api_key_placeholder", then uncomment the line
        // below to have Approov substitute the real key at runtime. See
        // SECRETS-PROTECTION.md.
        //ApproovService.AddSubstitutionHeader("Api-Key", null);

        // Tip (STAGE 2+): to attest on a device/emulator that is not registered via an
        // app signing certificate, obtain a dev key with `approov devkey -create` and add:
        //ApproovService.SetDevKey("<your-dev-key>");
        // Verbose Approov logging while testing:
        //ApproovService.SetLoggingLevel(ApproovLogLevel.Debug);

        // The Shapes API key travels on every request (checked by v1 and v5).
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Add("Api-Key", SHAPES_API_KEY);

        try
        {
            apiClient = RestService.For<IApiInterface>(httpClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during RestService: " + ex.Message);
        }
    }

    // "Hello" button — calls the unprotected /hello endpoint to check connectivity.
    private async void OnHelloButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var values = await apiClient.GetHello().ConfigureAwait(false);
            if (values != null && values.TryGetValue("text", out var text))
                UpdateUI("hello.png", text);
            else
                UpdateUI("confused.png", "Error getting Hello from Shapes server");
        }
        catch (Exception)
        {
            UpdateUI("confused.png", "Exception getting Hello from Shapes server");
        }
    }

    // "Shape" button — calls the /shapes endpoint that is protected by Approov
    // (from Stage 2 onward). A shape + "OK" means the request was accepted.
    private async void OnShapeButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var values = await apiClient.GetShape().ConfigureAwait(false);
            if (values != null && values.TryGetValue("shape", out var shape))
                UpdateUI(shape.ToLower() + ".png", "200 OK");
            else
                UpdateUI("confused.png", "Error getting Shape: response json malformed");
        }
        catch (Exception ex)
        {
            UpdateUI("confused.png", "Exception getting Shape: " + ex.Message);
        }
    }

    // Update the shape image and status label on the UI thread.
    private void UpdateUI(string imageSource, string message)
    {
        Dispatcher.Dispatch(() =>
        {
            logoImage.Source = imageSource;
            textMessage.Text = message;
        });
    }
}

// Refit REST interface. The paths are relative to httpClient.BaseAddress, which
// already carries the endpoint version (ENDPOINT_VERSION), so these never change
// as you move between stages.
public interface IApiInterface
{
    [Get("/hello/")]
    Task<Dictionary<string, string>> GetHello();
    [Get("/shapes/")]
    Task<Dictionary<string, string>> GetShape();
}
