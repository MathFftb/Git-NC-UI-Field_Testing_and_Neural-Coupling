using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.Multiplayer.Center.Common;
using UnityEngine;

/// <summary>
/// A TaskSettings Object contains all the settings of the current task running,
/// These informations then have to be registered along the data collected during the task to know what were the conditions of collection
/// Overseer should have a TaskSettings currentTask attribute to carry that information between settings selection and task run scenes
/// The Trial Settings scene UI should inform the attributes of the Task Settings object
/// </summary>
[Serializable]
public class TaskSettings
{
    [SerializeField] private PatientProfile.Side targetedSide;
    [SerializeField] private bool cooperativeOrNot;
    [SerializeField] private int cyclesMaxNumber;
    [SerializeField] private double cyclesDuration;
    [SerializeField] private int stimMaxNumber;
    [SerializeField] private double stimIntensity;

    [SerializeField] private DateTime taskDate;


    [SerializeField] private TaskPreset preset;
    // Getters and Setters

    [JsonConverter(typeof(StringEnumConverter))]
    public PatientProfile.Side TargetedSide { get => targetedSide; set => targetedSide = value; }

    public bool CooperativeOrNot { get => cooperativeOrNot; set => cooperativeOrNot = value; }

    public int CyclesMaxNumber { get => cyclesMaxNumber; set => cyclesMaxNumber = value; }

    public double CyclesDuration { get => cyclesDuration; set => cyclesDuration = value; }

    public int StimMaxNumber { get => stimMaxNumber; set => stimMaxNumber = value; }

    public double StimIntensity { get => stimIntensity; set => stimIntensity = value; }

    public DateTime TaskDate { get => taskDate; set => taskDate = value; }

    [JsonConverter(typeof(StringEnumConverter))]
    public TaskPreset Preset { get => preset; set => preset = value; }

    public TaskSettings(PatientProfile.Side _targetedSide, bool _cooperativeOrNot, int _cyclesMaxNumber, double _cyclesDuration, int _stimMaxNumber, double _stimIntensity)
    {
        this.targetedSide = _targetedSide;
        this.cooperativeOrNot = _cooperativeOrNot;
        this.cyclesMaxNumber = _cyclesMaxNumber;
        this.cyclesDuration = _cyclesDuration;
        this.stimMaxNumber = _stimMaxNumber;
        this.stimIntensity = _stimIntensity;

        this.preset = TaskSettings.TaskPreset.None;
    }

    public TaskSettings(TaskSettings another)
    {
        this.targetedSide = another.targetedSide;
        this.cooperativeOrNot = another.cooperativeOrNot;
        this.cyclesMaxNumber = another.cyclesMaxNumber;
        this.cyclesDuration = another.cyclesDuration;
        this.stimMaxNumber = another.stimMaxNumber;
        this.stimIntensity = another.stimIntensity;

        this.preset = another.preset;
    }

    public TaskSettings()
    {
        this.targetedSide = PatientProfile.Side.None;
        this.cooperativeOrNot = false;
        this.cyclesMaxNumber = 0;
        this.cyclesDuration = 0;
        this.stimMaxNumber = 0;
        this.stimIntensity = 0;

        this.preset = TaskSettings.TaskPreset.None;
    }

    #region Presets Definition

    public enum TaskPreset
    {
        None,
        Preset1,
        Preset2

    }

    // Constructors using Presets

    public TaskSettings(TaskSettings.TaskPreset selectedPreset)
    {
        switch (selectedPreset)
        {
            case TaskSettings.TaskPreset.None:
                this.targetedSide = PatientProfile.Side.None;
                this.cooperativeOrNot = false;
                this.cyclesMaxNumber = 0;
                this.cyclesDuration = 0;
                this.stimMaxNumber = 0;
                this.stimIntensity = 0;
                this.preset = TaskSettings.TaskPreset.None;
                break;
                
            /*
            case TaskSettings.TaskPreset.Preset1:
            this.targetedSide =         
            this.cooperativeOrNot =     
            this.cyclesMaxNumber =      
            this.cyclesDuration =       
            this.stimMaxNumber =        
            this.stimIntensity =        
            this.preset = TaskSettings.TaskPreset.Preset1;         
                break;
                */
        }
    }
    
    
    
    
    #endregion
}
