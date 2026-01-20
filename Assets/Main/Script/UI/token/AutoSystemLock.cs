using UnityEngine;

// 자동 시스템 간 충돌 방지를 위한 전역 락 클래스
// 
// AutoSpawn / AutoMerge가 동시에 실행되면서
// - 같은 오브젝트를 건드리거나
// - 중복 소환 / 중복 머지
// 가 발생하는 것을 방지하기 위한 용도
// 
// static으로 관리하여
// 씬 어디서든 동일한 상태를 공유한다.
public static class AutoSystemLock
{
    // 자동 합치기 실행 중 여부
    // - true일 때 AutoSpawner는 대기
    public static bool isAutoMerging = false;

    // 자동 소환 실행 중 여부
    // - true일 때 AutoMerger는 대기
    public static bool isAutoSpawning = false;
}