using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GFunc
{
    #region Print log func
    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message)
    {
#if DEBUG_MODE
        Debug.Log(message);
#endif      // DEBUG_MODE
    }       // Log()

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message, UnityEngine.Object context)
    {
#if DEBUG_MODE
        Debug.Log(message, context);
#endif      // DEBUG_MODE
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarning(object message)
    {
#if DEBUG_MODE
        Debug.LogWarning(message);
#endif      // DEBUG_MODE
    }       // Log()
    #endregion      // Print log func

    #region Assert for debug
    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Assert(bool condition)
    {
#if DEBUG_MODE
        Debug.Assert(condition);
#endif      // DEBUG_MODE
    }       // Assert()

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Assert(bool condition, object message)
    {
#if DEBUG_MODE
        Debug.Assert(condition, message);
#endif      // DEBUG_MODE
    }       // Assert()
    #endregion      // Assert for debug

    #region Vaild Func
    //! 컴포넌트의 유효성을 검사한다.
    public static bool IsValid<T>(this T component_) where T : Component
    {
        Component convert_ = (Component)(component_ as Component);
        bool isInvalid = convert_ == null || convert_ == default;
        return !isInvalid;
    }       // IsValid()

    //! 오브젝트의 유효성을 검사한다.
    public static bool IsValid(this GameObject obj_)
    {
        bool isInvalid = (obj_ == null || obj_ == default);
        return !isInvalid;
    }       // IsValid()

    //! 리스트의 유효성을 검사한다.
    public static bool IsValid<T>(this List<T> list_)
    {
        bool isInvalid = (list_ == null || list_ == default || list_.Count < 1);
        return !isInvalid;
    }       // IsValid()

    //! 리스트의 유효성을 검사한다.
    public static bool IsValid<T>(this List<T> list_, int index_)
    {
        bool isInvalid = (list_.IsValid() == false) ||
            (index_ < 0 || list_.Count <= index_);
        return !isInvalid;
    }       // IsValid()

    #endregion      // Vaild Func
}
