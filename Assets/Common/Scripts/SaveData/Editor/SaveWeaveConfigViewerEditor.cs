using UnityEditor;

[CustomEditor(typeof(SaveWeaveConfigViewer))]
public class SaveWeaveConfigViewerEditor 
    : SaveDataViewerEditorBase<SaveWeaveConfigViewer, SaveDataWeaveConfig.Data>
{
    protected override string Label => "WeaveConfig";
    protected override void Load() => viewer.data = SaveDataWeaveConfig.Load();
    protected override void Save() => SaveDataWeaveConfig.Save(viewer.data);
}