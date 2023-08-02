using UnityEngine;

public static class Factory
{
    public static IItem CreatePopUp(GameObject newObject)
    {
        return new NewPopUp(newObject);
    }

    public static IItem CreatePopUpLoseHeart(GameObject newObject)
    {
        return new NewPopUpLoseHeart(newObject);
    }
}