using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData 
{
    public string email;
    public string password;
    public string name;
    public Gender gender;

    public string description = "I am a cool person";
    public string avatarUrl = "default_avatar";



}

public enum Gender{
    Male,
    Female,
    Lesbian,
    Gay,
    Bisexual,
    Transgender,
}
