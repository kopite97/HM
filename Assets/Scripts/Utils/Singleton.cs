using UnityEngine;

// <T>는 상속받을 클래스 이름입니다.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    
    // 쓰레드 안전성을 위한 락 (선택사항이나 안전을 위해 추가)
    private static readonly object _lock = new object();
    
    // 게임 종료 시 접근 방지용 플래그
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                Debug.LogWarning($"[Singleton] 게임이 종료되는 중입니다. '{typeof(T)}' 인스턴스를 생성하지 않습니다.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 1. 씬에 이미 존재하는지 찾기
                    _instance = (T)FindFirstObjectByType(typeof(T));

                    if (FindFirstObjectByType(typeof(T)))
                    {
                        return _instance;
                    }

                    // 2. 없으면 새로 만들기
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // 3. 씬 전환 시 파괴되지 않게 설정
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // 중복된 인스턴스가 생성되려 하면 파괴
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    public virtual void Initialize()
    {
        
    }

}