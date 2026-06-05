using Cysharp.Threading.Tasks;
/// <summary>
/// Firebase Realtime Database 기반 저장소.
/// 로컬(PlayerPrefs)에 즉시 캐시 + 백그라운드로 Firebase에 비동기 업로드.
/// </summary>
public class FirebaseSaveStorage : ISaveStorage
{
    private readonly PlayerPrefsSaveStorage _local = new();

    public void Save(SaveData data) => _local.Save(data);

    public SaveData Load() => _local.Load();

    public void Sync() => Managers.FirebaseM?.WriteData().Forget();
}
