using System;
using GraphicsAPI;
using GraphicsAPI.CustomPostProcessing;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace ExampleEnemy;

public class PostProcessVisualsFreddy: MonoBehaviour
{
    
    [SerializeField] public FullScreenCustomPass fullScreenPass;
    
    
    
    public static PostProcessVisualsFreddy Instance { get; private set; }
    private void Awake()
    {
        if ((UnityEngine.Object) PostProcessVisualsFreddy.Instance == (UnityEngine.Object) null)
            PostProcessVisualsFreddy.Instance = this;
        else
            UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    private void Start()
    {
        if (CustomPostProcessingManager.Initialized)
        {
            PostProcess blackAndWhite = new PostProcess("BlackAndWhite", Plugin.FreddyModAssets.LoadAsset<Material>("Asset/Freddy/BWShader.mat"))
            {
                InjectionType = InjectionType.AfterPostProcess,
                Enabled = true
            };
            this.fullScreenPass = CustomPostProcessingManager.Instance.AddPostProcess(blackAndWhite);
        }
        else
        {
            CustomPostProcessingManager.OnLoad += new EventHandler(this.Cpp_OnLoad);
        }
    }

    private void Cpp_OnLoad(object sender, EventArgs e)
    {
        Debug.Log("PRESENT IN THE LOAD");
        PostProcess blackAndWhite = new PostProcess("BlackAndWhite", Plugin.FreddyModAssets.LoadAsset<Material>("BWShader"))
        {
            InjectionType = InjectionType.AfterPostProcess,
            Enabled = true
        };
        this.fullScreenPass = CustomPostProcessingManager.Instance.AddPostProcess(blackAndWhite);
        CustomPostProcessingManager.OnLoad -= new EventHandler(this.Cpp_OnLoad);
    }
    //Example on how to blink
    /*public IEnumerator Blink()
    {
      this.blinking = true;
      yield return (object) new WaitUntil((Func<bool>) (() => (double) this.blinkingTime == 1.0));
      yield return (object) new WaitForSeconds(0.3f);
      this.blinking = false;
    }
    */
}