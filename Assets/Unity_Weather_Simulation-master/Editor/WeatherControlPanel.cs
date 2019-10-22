using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BasicWeather
{
    public bool enabled = false;
    public float density = 0.00f;

    public void init(string label)
    {

        this.enabled = EditorGUILayout.BeginToggleGroup(label, this.enabled);
        show();
        EditorGUILayout.EndToggleGroup();
    }
    protected virtual void show()
    {

        density = EditorGUILayout.Slider("Density", density, 0, 100);


    }
    public virtual void reset()
    {
        this.enabled = false;//1
        this.density = 0.00f;
    }

}

public class Precipitation : BasicWeather
{
    public float speed = 20.00f;
    public float turbulence = 0.00f;

    protected override void show()
    {
        base.show();
        speed = EditorGUILayout.Slider("Speed", speed, 0, 100);
        turbulence = EditorGUILayout.Slider("Turbulence", turbulence, 0, 100);
    }
}

public class Mist : BasicWeather
{
    public float height = 0.00f;
    public Color color = Color.white;
    protected override void show()
    {
        base.show();
        height = EditorGUILayout.Slider("Height", height, 0, 100);
        this.color = EditorGUILayout.ColorField("Color", this.color);

    }
    public override void reset()
    {
        base.reset();
        this.color = Color.white;
        this.height = 0.00f;
    }

}

public class Fog : BasicWeather
{

    public Color color = Color.white;
    protected override void show()
    {
        base.show();

        this.color = EditorGUILayout.ColorField("Color", this.color);

    }
    public override void reset()
    {
        base.reset();
        this.color = Color.white;
       
    }

}

public class Cloud : BasicWeather
{
    public float height = 0.00f;
    public Color color = Color.white;
    protected override void show()
    {
        base.show();
        height = EditorGUILayout.Slider("Height", height, 0, 100);
        this.color = EditorGUILayout.ColorField("Color", this.color);

    }
    public override void reset()
    {
        base.reset();
        this.color = Color.white;
        this.height = 0.00f;
    }

}

public class Hail : BasicWeather
{
    public float speed = 20.00f;

    protected override void show()
    {
        base.show();
        speed = EditorGUILayout.Slider("Speed", speed, 0, 100);
    }
}


public class WeatherControlPanel : EditorWindow
{
    static Precipitation rain = new Precipitation();
    static Precipitation snow = new Precipitation();
    static Hail hail = new Hail();
    static BasicWeather sun = new BasicWeather();
    static Mist mist = new Mist();
    static Cloud cloud = new Cloud();
    static Fog fog = new Fog();

    private float windD;
    private float windI;
    private float time;
    private bool foldout = false;
    private Camera targetCamera = null;
    private GameObject go;
    private WeatherObject wo;
    private static WeatherControlPanel window;


    public int resHeight = 2550;
    public int resWidth = 3300;
    public string customFilename = "";

    TextAnchor defaultAlignment;

    [MenuItem("Weather System/Weather Control Panel", priority = 0)]
    static void Init()
    {

        window = (WeatherControlPanel)EditorWindow.GetWindow(typeof(WeatherControlPanel));
        window.Show();

    }

    [MenuItem("Weather System/Reset", priority = 11)]
    static void reset()
    {
        rain.reset();
        snow.reset();
        hail.reset();
        sun.reset();
        mist.reset();
        cloud.reset();
        fog.reset();
    }

    public WeatherControlPanel()
    {

        minSize = new Vector2(360, 580);
        rain.density = 50f;
        snow.density = 50f;
        hail.density = 20f;;
        snow.turbulence = 10f;
        Debug.Log("constructing");

    }

    private void OnEnable()
    {
        wo = (WeatherObject)GameObject.Find("WeatherObject").GetComponent(typeof(WeatherObject));
        if (!wo)
        {
            EditorUtility.DisplayDialog("Error", "Weather Object Not Found!\nPlease create a weather object before you proceed!", "Exit");
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            //EditorApplication.Exit(0);
        }
        wo.init();
    }

    private void link(WeatherObject obj)

    {
        //Clear particles if not checked
        if (rain.enabled)
        {
            obj.set_rainDensity(rain.density);
        }
        else
        {
            obj.set_rainDensity(0f);
        }

        if (snow.enabled)
        {
            obj.set_snowDensity(snow.density);
        }
        else
        {
            obj.set_snowDensity(0f);
        }

        if (hail.enabled)
        {
            obj.set_hailDensity(hail.density);
        }
        else
        {
            obj.set_hailDensity(0f);
        }
        if (mist.enabled)
        {
            obj.set_mist(mist.color, mist.density);
        }
        else
        {
            obj.set_mist(mist.color, 0f);
        }

        if (fog.enabled)
        {
            obj.set_fogDensity(fog.density);
        }
        else
        {
            obj.set_fogDensity(0f);
        }
        if (cloud.enabled)
        {
            obj.enableCloud();
            obj.set_cloudDensity(cloud.density);
        }
        else
        {
            obj.disableCloud();
        }
        obj.set_Wind(windD);
        obj.set_WindIntensity(windI);
        obj.set_rainSpeed(rain.speed);
        obj.set_rainTurbulence(rain.turbulence);
        obj.set_snowTurbulence(snow.turbulence);
        obj.set_snowSpeed(snow.speed);
        obj.set_hailSpeed(hail.speed);
        obj.set_fogColor(fog.color);
        obj.set_fogDensity(fog.density);
        obj.set_mistHeight(mist.height);
        obj.set_cloudColor(cloud.color);
        obj.set_cloudHeight(cloud.height);
        obj.updateLight(time);
    }

    void OnGUI()
    {

        rain.init("Rain");
        snow.init("Snow");
        hail.init("Hail");
        mist.init("Mist");
        fog.init("Fog");
        cloud.init("Cloud");
        sun.init("Sun");
        link(wo);
        if (rain.enabled == false)
        {
            wo.disableRain();
        }



        GUILayout.Space(20);
        GUILayout.Label("Global Setting", EditorStyles.boldLabel);
        GUILayout.Space(5);

       
         //Wind control
        windD = EditorGUILayout.Slider("Wind Direction", windD, -180, 180);
        windI = EditorGUILayout.Slider("Wind Intensity", windI, 0, 100);

        //Day and night cycle control
        defaultAlignment = GUI.skin.label.alignment;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Space(10);
        GUILayout.Label("Time of the Day");
        GUI.skin.label.alignment = defaultAlignment;
        time = GUILayout.HorizontalSlider(time, 0, 24);
        GUILayout.BeginHorizontal();
        defaultAlignment = GUI.skin.label.alignment;
        GUILayout.Label("Day");
        GUI.skin.label.alignment = TextAnchor.UpperRight;
        GUILayout.Label("Night");
        GUI.skin.label.alignment = defaultAlignment;
        GUILayout.EndHorizontal();
        GUILayout.Space(5);


        //Screenshot functionality
        foldout = EditorGUILayout.Foldout(foldout, "Screenshot");
        if (foldout)
        {

            go = (GameObject)EditorGUILayout.ObjectField("Select a camera:", go, typeof(GameObject), true);
            if (!(go && (targetCamera = go.GetComponent<Camera>())))
            {

                ShowNotification(new GUIContent("No Camera Selected"));

            }
            else
            {
                RemoveNotification();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Resolution(Heigh*Width):"), GUILayout.MaxWidth(150));
            resHeight = EditorGUILayout.IntField(resHeight, GUILayout.MaxWidth(160));
            EditorGUILayout.LabelField(new GUIContent("*"), GUILayout.MaxWidth(5));
            resWidth = EditorGUILayout.IntField(resWidth, GUILayout.MaxWidth(160));
            EditorGUILayout.EndHorizontal();

            customFilename = EditorGUILayout.TextField("Filename:", customFilename);
            if (GUILayout.Button("Capture"))
            {
                if (targetCamera)
                {
                    if ((resHeight < 1) || (resWidth < 1))
                    {
                        EditorUtility.DisplayDialog("Error", "Invalid resolution!", "OK");
                    }
                    else
                    {
                        Capture();
                    }


                }

            }
        }
        else
        {
            RemoveNotification();

        }

    }

    public static string ScreenShotName(int width, int height, string userFilename)
    {
        if (userFilename == "")
        {
            return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
            Application.dataPath,
            width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }
        else
        {
            return string.Format("{0}/screenshots/{1}_{2}.png",
            Application.dataPath,
            userFilename,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

    }

    public void Capture()
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        targetCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        targetCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        targetCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight, customFilename);
        System.IO.FileInfo file = new System.IO.FileInfo(filename);
        file.Directory.Create(); // If the directory already exists, this method does nothing.
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }
}
