using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class SampleSceneSmokeTests
{
    [UnityTest]
    public IEnumerator SampleScene_Loads_And_HasSceneContextWithCriticalServices()
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

        yield return null;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);

        var sceneContext = Object.FindObjectOfType<SceneContext>();
        Assert.IsNotNull(sceneContext, "Missing SceneContext in SampleScene.");

        Assert.IsNotNull(sceneContext.PlayerInput, "SceneContext.PlayerInput is not set.");
        Assert.IsNotNull(sceneContext.UiCanvas, "SceneContext.UiCanvas is not set.");

        Assert.IsNotNull(sceneContext.Get<TimeManager>(), "Missing TimeManager (or not wired in SceneContext).");
        Assert.IsNotNull(sceneContext.Get<WeatherManager>(), "Missing WeatherManager (or not wired in SceneContext).");
        Assert.IsNotNull(sceneContext.Get<InventoryHotbarController>(), "Missing InventoryHotbarController (or not wired in SceneContext).");
        Assert.IsNotNull(sceneContext.Get<InventoryContainer>(), "Missing InventoryContainer (or not wired in SceneContext).");
    }
}
