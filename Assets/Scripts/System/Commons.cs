using System;

//--------------------------------------------------------------
//
//  ���ʃV�X�e�������N���X
//
//--------------------------------------------------------------
public class Commons
{
    /// <summary>
    /// �w�肳�ꂽ�񋓌^�̒l�̗v�f����Ԃ��܂�
    /// </summary>
    public static int GetEnumLength<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }
}