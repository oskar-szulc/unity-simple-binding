using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PropertyBinding))]
public class PropertyBindingInspector : Editor
{ 
    [SerializeField] private int _sourceIndex;
    [SerializeField] private int _targetIndex;

    [SerializeField] private string[] _sourcePropertiesNames;
    [SerializeField] private string[] _targetPropertiesNames;

    [SerializeField] private List<PropertyInfo> _sourceProperties = new List<PropertyInfo>();
    [SerializeField] private List<PropertyInfo> _targetProperties = new List<PropertyInfo>();

    [SerializeField] private string _formatString = "{0}";

    private PropertyBinding _bindingClass;
    private bool _refreshed;
    void OnEnable()
    {
        _refreshed = false;
        _bindingClass = target as PropertyBinding;

        _sourceIndex = _bindingClass.SourcePropertyIndex;
        _targetIndex = _bindingClass.TargetPropertyIndex;
        _formatString = _bindingClass.FormatString;

        RefreshSourceProperties();

        if (_sourceProperties.Count != 0)
        {
            if (_sourceIndex >= _sourceProperties.Count) _sourceIndex = 0;
            _bindingClass.SourceProperty = _sourceProperties[_sourceIndex];
            _bindingClass.SourcePropertyName = _sourceProperties[_sourceIndex].Name;

            _sourcePropertiesNames = _sourceProperties.Select(p => p.Name).ToArray();

            RefreshTargetProperties();
            if (_targetProperties.Count != 0)
            {
                if (_targetIndex >= _targetProperties.Count) _targetIndex = 0;
                _bindingClass.TargetProperty = _targetProperties[_targetIndex];
                _bindingClass.TargetPropertyName = _targetProperties[_targetIndex].Name;

                _targetPropertiesNames = _targetProperties.Select(p => p.Name).ToArray();
                _refreshed = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!_refreshed && _bindingClass.Source != null && _bindingClass.Target != null)
        {
            _refreshed = true;
            Refresh();
        }

        if (GUILayout.Button("Refresh properties"))
        {
            Refresh();
        }

        if (_sourceProperties.Count > 0)
        {
            _sourceIndex = EditorGUILayout.Popup("Source: ", _sourceIndex, _sourcePropertiesNames);
        }
        else
        {
            GUILayout.Label("No source properties");
        }

        if (_targetProperties.Count > 0)
        {
            _targetIndex = EditorGUILayout.Popup("Target: ", _targetIndex, _targetPropertiesNames);

            if (_targetProperties[_targetIndex].PropertyType == typeof (string))
            {
                _formatString = EditorGUILayout.TextField("Format string: ", _formatString);
            }
        }
        else
        {
            GUILayout.Label("No target properties");
        }

        if (GUI.changed)
        {
            _bindingClass.SourcePropertyIndex = _sourceIndex;
            _bindingClass.TargetPropertyIndex = _targetIndex;
             
            if(_sourceIndex < _sourceProperties.Count)
                _bindingClass.SourceProperty = _sourceProperties[_sourceIndex];

            if(_targetIndex < _targetProperties.Count)
                _bindingClass.TargetProperty = _targetProperties[_targetIndex];

            _bindingClass.FormatString = _formatString;
        }

        EditorUtility.SetDirty(target);
    }

    private void Refresh()
    {
        RefreshSourceProperties();
        RefreshTargetProperties();

        _sourcePropertiesNames = _sourceProperties.Select(p => p.Name).ToArray();
        _targetPropertiesNames = _targetProperties.Select(p => p.Name).ToArray();
    }

    private void RefreshSourceProperties()
    {
        if (_bindingClass.Source == null) return;

        var tempSource = from sourceProp in _bindingClass.Source.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                         select sourceProp;

        _sourceProperties = tempSource.ToList();
    }

    private void RefreshTargetProperties()
    {
        if (_bindingClass.Target == null || _bindingClass.Source == null) return;

        var tempTarget = from targetProp in _bindingClass.Target.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                         where targetProp.PropertyType.IsAssignableFrom(_bindingClass.SourceProperty.PropertyType) || targetProp.PropertyType == typeof(string)
                         select targetProp;

        _targetProperties = tempTarget.ToList();
    }
}
