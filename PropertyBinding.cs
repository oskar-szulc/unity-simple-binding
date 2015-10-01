using System.Reflection;
using UniRx;
using UnityEngine;

public class PropertyBinding : MonoBehaviour
{
    [HideInInspector] public int            SourcePropertyIndex;
    [HideInInspector] public int            TargetPropertyIndex;
    [HideInInspector] public PropertyInfo   SourceProperty;
    [HideInInspector] public PropertyInfo   TargetProperty;
    [HideInInspector] public string         SourcePropertyName;
    [HideInInspector] public string         TargetPropertyName;

    public string           FormatString;
    public MonoBehaviour    Source;
    public MonoBehaviour    Target;

    void Start()
    {
        #if !UNITY_EDITOR
        var tempTarget = from targetProp in Target.GetType().GetProperties()
                         where targetProp.Name == TargetPropertyName
                         select targetProp;

        var tempSource = from sourceProp in Source.GetType().GetProperties()
                         where sourceProp.Name == SourcePropertyName
                         select sourceProp;

        TargetProperty = tempTarget.FirstOrDefault();
        SourceProperty = tempSource.FirstOrDefault();
        #endif

        SourceProperty.ObserveEveryValueChanged(x => x.GetValue(Source, null)).Subscribe(UpdateTargetProperty);
    }

    private void UpdateTargetProperty(object value)
    {
        if (TargetProperty.PropertyType == typeof(string))
        {
            TargetProperty.SetValue(
                Target,
                string.Format(FormatString, value),
                null
            );
        }
        else //we can assign 1:1
        {
            TargetProperty.SetValue(
                Target,
                value,
                null
            );
        }
    }
}
