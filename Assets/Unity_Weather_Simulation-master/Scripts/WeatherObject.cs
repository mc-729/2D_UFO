using UnityEngine;
using UnityEditor;
using System;
using System.Collections;




public class WeatherObject : MonoBehaviour
{
    [Header("Rain Settings")]
    public float RainDensityThreshold = 1000;
    public float RainNoiseThreshold = 0.01f;
    public float RainWindResistence = 0.15f;
    [Header("Snow Settings")]
    public float SnowDensityThreshold = 1000;
    public float SnowNoiseThreshold = 0.2f;
    public float SnowWindResistence = 0.25f;
    [Header("Hail Settings")]
    public float HailDensityThreshold = 500;
    public float HailWindResistence = 1.0f;
    [Header("Mist Settings")]
    public float MistDensityThreshold = 100;
    public float MistHeightThreshold = 30;
    [Header("Fog Settings")]
    public float FogDensityThreshold = 0.5f;
    [Header("Cloud Settings")]
    public float CloudHeightThreshold = 30f;
    [Header("General")]
    public float SunIntensityThreshold = 1.5f;
    public float GravityConst = 10f;
    public float GravityRatio = 0.5f;//contantMax/constantMin



    private Vector2 baseVector = new Vector2(1, 0);
    private Vector2 windDirection;
    private float windIntensity = 0f;
    private bool profileSaved = false;//mist color

    Vector3 rainSize;
    Vector3 snowSize;
    Vector3 hailSize;
    Vector3 mistSize;
    Vector3 cloudSize;
   


    GradientAlphaKey[] minKeys = null;
    GradientAlphaKey[] maxKeys = null;





    private ParticleSystem rain = null; //cannot inherit from particlesystem
    private ParticleSystem snow = null;
    private ParticleSystem hail = null;
    private ParticleSystem mist = null;
    private ParticleSystem cloud = null;
    private Light sun;
    private bool isInited = false;

    private void initCheck()
    {
        if (!isInited)
        {
            init();
            isInited = true;
        }
    }
    private void Awake()
    {
        initCheck();
    }

    public void init()
    {
        rainSize = new Vector3(70, 1, 70);
        snowSize = new Vector3(100, 2.5f, 100);
        hailSize = new Vector3(1, 1, 1);
        mistSize = new Vector3(25, 15, 15);
        cloudSize = new Vector3(180, 10, 180);
        minKeys = new GradientAlphaKey[8];
        maxKeys = new GradientAlphaKey[4];
        maxKeys[0].time = 0;
        maxKeys[0].alpha = 0;
        maxKeys[1].time = 0.25f;
        maxKeys[1].alpha = 100;
        maxKeys[2].time = 0.75f;
        maxKeys[2].alpha = 50;
        maxKeys[3].time = 0;
        maxKeys[3].alpha = 0;
        minKeys[0].time = 0;
        minKeys[0].alpha = 0;
        minKeys[1].time = 0.5f;
        minKeys[1].alpha = 50;
        minKeys[2].time = 1;
        minKeys[2].alpha = 0;
        rain = (ParticleSystem)this.transform.Find("RainParticle").GetComponent(typeof(ParticleSystem));
        snow = (ParticleSystem)this.transform.Find("SnowParticle").GetComponent(typeof(ParticleSystem));
        hail = (ParticleSystem)this.transform.Find("HailParticle").GetComponent(typeof(ParticleSystem));
        mist = (ParticleSystem)this.transform.Find("MistParticle").GetComponent(typeof(ParticleSystem));
        cloud = (ParticleSystem)this.transform.Find("CloudParticle").GetComponent(typeof(ParticleSystem));
        sun = this.transform.Find("Sun").GetComponent<Light>();
        if (snow == null || rain == null || hail == null || mist == null || cloud == null)
        {
            EditorUtility.DisplayDialog("Error", "The weather object is corrupted, please reinstall!", "Exit");
            //EditorApplication.Exit(0);
        }
        applyWind();
    }

    public void adjustSize(float scale)
    {
        Scale(rain, scale,rainSize);
        Scale(snow, scale,snowSize);
        Scale(hail, scale,hailSize);
        Scale(mist, scale,mistSize);
        Scale(cloud, scale,cloudSize);
    }
    private void Scale(ParticleSystem ps, float value,Vector3 baseSzie)
    {
        
        var temp = ps.shape;
        var size = temp.scale;

        size.x = value * baseSzie.x;
        size.y = value * baseSzie.y;
        size.z = value * baseSzie.z;
        Debug.Log(rainSize.x);
        temp.scale = size;
    }

    //rate over life
    private void setROT(int typeCode, float rate_ROT)

    {
        initCheck();
        ParticleSystem.EmissionModule em;
        switch (typeCode)
        {
            case 0:
                em = rain.emission;
                em.rateOverTime = RainDensityThreshold * (rate_ROT / 100);
                break;
            case 1:
                em = snow.emission;
                em.rateOverTime = SnowDensityThreshold * (rate_ROT / 100);
                break;
            case 2:
                em = hail.emission;
                em.rateOverTime = HailDensityThreshold * (rate_ROT / 100);
                break;
            case 3:
                em = mist.emission;
                em.rateOverTime = MistDensityThreshold * (rate_ROT / 100);
                break;
           
            default:
                Debug.Log("Invalid weather type!");
                break;
        }

    }

    private void setGravity(int typeCode, float speed)
    {
        initCheck();
        float multiplier = 0f;
        ParticleSystem.MinMaxCurve temp;
        ParticleSystem.MainModule main;
        switch (typeCode)
        {
            case 0:
                main = rain.main;
                temp = rain.main.gravityModifier;
                temp.mode = ParticleSystemCurveMode.Constant;
                temp.constant = speed / 10;
                main.gravityModifier = temp;
                break;
            case 1:
                main = snow.main;
                multiplier = 0.35f;
                temp = snow.main.gravityModifier;
                temp.mode = ParticleSystemCurveMode.TwoConstants;
                temp.constantMax = GravityConst * multiplier * (speed / 100);
                temp.constantMin = temp.constantMax * GravityRatio;
                main.gravityModifier = temp;
                break;
            case 2:
                main = hail.main;
                multiplier = 30f;
                temp = hail.main.gravityModifier;
                temp.mode = ParticleSystemCurveMode.TwoConstants;
                temp.constantMax = GravityConst * multiplier * (speed / 100);
                temp.constantMin = temp.constantMax * GravityRatio;
                main.gravityModifier = temp;
                break;
            default:
                Debug.Log("Invalid weather type!");
                break;
        }

    }

    private void setTurbulence(int typeCode, float value)
    {
        initCheck();
        ParticleSystem.MinMaxCurve temp;
        ParticleSystem.NoiseModule n; 
        ParticleSystem.MinMaxCurve s; 
        switch (typeCode)
        {
            case 0:
                n = rain.noise;
                s = n.strength;
                temp = rain.noise.strength;
                temp.mode = ParticleSystemCurveMode.TwoConstants;
                temp.constantMax = value * RainNoiseThreshold;
                temp.constantMax = 0.5f * temp.constantMax;
                s = temp;
                n.strength = temp;
                break;
            case 1:
                n = snow.noise;
                s = n.strength;
                temp = snow.noise.strength;
                temp.mode = ParticleSystemCurveMode.TwoConstants;
                temp.constantMax = value * SnowNoiseThreshold;
                temp.constantMin = 0.2f * temp.constantMax;
                s = temp;
                n.strength = temp;
                break;

            default:
                Debug.Log("Invalid weather type!");
                break;
        }

    }

    public void disableRain()
    {
        rain.Clear();
    }
    public void enableCloud()
    {
        var temp = cloud.emission;
        temp.enabled = true;

    }
    public void disableCloud()
    {
        var temp = cloud.emission;
        temp.enabled = false;
        cloud.Clear();
    }


    public void set_fogColor(Color color)
    {
        if(RenderSettings.fog)
            RenderSettings.fogColor = color;
    }



    private void setWind(ParticleSystem.ForceOverLifetimeModule fol, float sen)
    {
        fol.enabled = true;
        float ratio = (windIntensity * sen) / Mathf.Sqrt((windDirection.x * windDirection.x) + (windDirection.y * windDirection.y));
        fol.x = windDirection.x * ratio;
        fol.z = windDirection.y * ratio;
    }
    private void applyWind()
    {
        setWind(rain.forceOverLifetime, RainWindResistence);
        setWind(snow.forceOverLifetime, SnowWindResistence);
        setWind(hail.forceOverLifetime, HailWindResistence);


    }
    public void set_WindIntensity(float newValue)
    {
        windIntensity = newValue;
        applyWind();
    }

    public void set_Wind(float degree)
    {

        windDirection.x = baseVector.x * (Mathf.Cos((degree * Mathf.PI / 180))) + baseVector.y * (Mathf.Sin((degree * Mathf.PI / 180)));
        windDirection.y = -baseVector.x * (Mathf.Sin((degree * Mathf.PI / 180))) + baseVector.y * (Mathf.Cos((degree * Mathf.PI / 180)));
        applyWind();


    }

    //setter apis


    public void set_rainDensity(float newValue)
    {
        if (newValue > 100) newValue = 100;
        setROT(0, newValue);
    }
    public void set_rainSpeed(float newValue)
    {
        if (newValue > 100) newValue = 100;
        setGravity(0, newValue);
    }
    public void set_rainTurbulence(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        setTurbulence(0, newValue);
    }

    public void set_snowDensity(float newValue)
    {
        if (newValue > 100) newValue = 100;
        setROT(1, newValue);
    }
    public void set_snowSpeed(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        setGravity(1, newValue);
    }
    public void set_snowTurbulence(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        setTurbulence(1, newValue);
    }

    public void set_hailDensity(float newValue)
    {
        if (newValue > 100) newValue = 100;
        setROT(2, newValue);
    }

    public void set_hailSpeed(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        setGravity(2, newValue);
    }

    public void set_fogDensity(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue <= 0) newValue = 0;
        if (!RenderSettings.fog)
            RenderSettings.fog = true;
        RenderSettings.fogDensity = (float)0.5 * (newValue / 100);
    }

    public void set_mist(Color color, float density)
    {
        //if (!profileSaved)
       // {
       //     maxKeys = mist.colorOverLifetime.color.gradientMax.alphaKeys;
       //     minKeys = mist.colorOverLifetime.color.gradientMin.alphaKeys;
       //     profileSaved = true;
      //  }
        
        var temp = mist.colorOverLifetime;
        var colorTemp = temp.color;
        Gradient colorMaxGrad = new Gradient();
        Gradient colorMinGrad = new Gradient();
        GradientColorKey[] keys = mist.colorOverLifetime.color.gradientMax.colorKeys;
        GradientAlphaKey[] alphaMax = mist.colorOverLifetime.color.gradientMax.alphaKeys;
        GradientAlphaKey[] alphaMin = mist.colorOverLifetime.color.gradientMin.alphaKeys;
        for (int i = 0; i < alphaMax.Length; i++)
            alphaMax[i].alpha = maxKeys[i].alpha * (density / 10000);
        for (int i = 0; i < alphaMin.Length; i++)
            alphaMin[i].alpha = minKeys[i].alpha * (density / 10000);
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].color = color;
        }
        colorMaxGrad.SetKeys(keys, alphaMax);
        colorMinGrad.SetKeys(keys, alphaMin);
        colorTemp.gradientMax = colorMaxGrad;
        colorTemp.gradientMin = colorMinGrad;
        temp.color = colorTemp;

    }
   
    public void set_mistHeight(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        var temp = mist.shape;
        var pos = temp.position;
        pos.y = MistHeightThreshold * (newValue / 100);
        temp.position = pos;

    }
    public void set_cloudDensity(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        var temp = cloud.main;
        temp.startSize = 60 + 20 * (newValue / 10000);

    }
  
    public void set_cloudHeight(float newValue)
    {
        if (newValue > 100) newValue = 100;
        if (newValue < 0) newValue = 0;
        var temp = cloud.shape;
        var pos = temp.position;
        var update = 70 + CloudHeightThreshold * (newValue / 100);
        if(Math.Abs(pos.y -  update) > 1)//clear old particles when updating height
            cloud.Clear();
        pos.y = update;
        temp.position = pos;


    }
    public void set_cloudColor(Color color)
    {

        var temp = cloud.colorOverLifetime;
        var colorTemp = temp.color;
        Gradient colorMinGrad = new Gradient();
        Gradient colorMaxGrad = new Gradient();
        GradientColorKey[] keys = cloud.colorOverLifetime.color.gradientMax.colorKeys;
        for (int i = 0; i < keys.Length; i++)
            keys[i].color = color;
        colorMinGrad.SetKeys(keys, cloud.colorOverLifetime.color.gradientMin.alphaKeys);
        colorMaxGrad.SetKeys(keys, cloud.colorOverLifetime.color.gradientMax.alphaKeys);
        colorTemp.gradientMin = colorMinGrad;
        colorTemp.gradientMax = colorMaxGrad;
        temp.color = colorTemp;

    }
    public void updateLight(float time)
    {
        if (time <= 20 && time >= 6)
        {
            var temp = sun.transform.position;
            temp.z = -1000f + 2000f * ((time - 6f) / 14);
            sun.transform.position = temp;
            sun.intensity = SunIntensityThreshold- (1.5f * Math.Abs(time - 12)/8);
        }
        else
        {
            sun.intensity = 0;
        }
        //1 , 0.987, 0.85 : 1, 00.51 , 0.23

    }

    //other apis
    public void get_screenshot()
    {
        GameObject go = GameObject.Find("Canvas");
        if (go)
        {
            go.SetActive(false);//hide canvas
            ScreenCapture.CaptureScreenshot("WeatherSystem" + (DateTime.Now).ToString("MMddHHmmss") + ".png");
            go.SetActive(true);
        }

    }

}
