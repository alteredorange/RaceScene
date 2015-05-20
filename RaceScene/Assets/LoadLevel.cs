using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadLevel : MonoBehaviour
{

  public GameObject mainImage;
  public Slider sliderBar;
  public GameObject LoadImage;


  private AsyncOperation async;

  public void ClickLoadLevel(int level)
  {
    mainImage.SetActive(false);
    LoadImage.SetActive(true);
    StartCoroutine(LoadLevelSliderBar(level));
  }


  IEnumerator LoadLevelSliderBar(int level)
  {
    async = Application.LoadLevelAsync(level);
    while (!async.isDone)
    {
      sliderBar.value = async.progress;
      yield return null;
    }
  }
}
