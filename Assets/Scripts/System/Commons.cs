using System;

//--------------------------------------------------------------
//
//  共通システム処理クラス
//
//--------------------------------------------------------------
public class Commons
{
    /// <summary>
    /// 指定された列挙型の値の要素数を返します
    /// </summary>
    public static int GetEnumLength<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }
}