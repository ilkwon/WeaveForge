using UnityEditor;

[CustomEditor(typeof(SaveDataInfoViewer))]
public class SaveDataInfoViewerEditor 
    : SaveDataViewerEditorBase<SaveDataInfoViewer, SaveData.Info>
{
    private string _path => 
        UnityEngine.Application.persistentDataPath + "/bin00.root";
    protected override string Label => "SaveData.Info";
    protected override void Load() => SaveData.Load(_path, ref viewer.data);
    protected override void Save() => SaveData.Save(_path, viewer.data);
}