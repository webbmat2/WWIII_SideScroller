using UnityEngine;
using UnityEditor;

public class ChapterDataCreator : EditorWindow
{
    [MenuItem("WWIII/Create Chapter Data Assets")]
    public static void ShowWindow()
    {
        GetWindow<ChapterDataCreator>("Chapter Data Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("WWIII Chapter Data Creator", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create All 7 Chapter Assets"))
        {
            CreateAllChapterAssets();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Meadowbrook Park"))
        {
            CreateMeadowbrookPark();
        }
        
        if (GUILayout.Button("Create Torch Lake"))
        {
            CreateTorchLake();
        }
        
        if (GUILayout.Button("Create Notre Dame"))
        {
            CreateNotreDame();
        }
        
        if (GUILayout.Button("Create High School"))
        {
            CreateHighSchool();
        }
        
        if (GUILayout.Button("Create Philadelphia"))
        {
            CreatePhiladelphia();
        }
        
        if (GUILayout.Button("Create Parson's Chicken"))
        {
            CreateParsonsChicken();
        }
        
        if (GUILayout.Button("Create Costa Rica"))
        {
            CreateCostaRica();
        }
    }

    private void CreateAllChapterAssets()
    {
        CreateMeadowbrookPark();
        CreateTorchLake();
        CreateNotreDame();
        CreateHighSchool();
        CreatePhiladelphia();
        CreateParsonsChicken();
        CreateCostaRica();
        
        Debug.Log("Created all 7 chapter data assets!");
        AssetDatabase.Refresh();
    }

    private void CreateMeadowbrookPark()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "meadowbrook-park";
        chapter.title = "Meadowbrook Park";
        chapter.location = "Northville";
        chapter.musicTag = "tutorial-theme";
        chapter.objectives = new string[] 
        {
            "Learn hose controls",
            "Cross the Slip-n-Slide gate",
            "Defeat Purple Pig (Kristen)"
        };
        chapter.hasBoss = true;
        chapter.bossType = "PurplePig";
        chapter.availablePowerUps = new PowerUpType[] { PowerUpType.Hose, PowerUpType.CherryPie };
        chapter.collectibleName = "Golden Fried ush Signs";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Tutorial level - introduces hose mechanics",
            "Slip-n-Slide gate must be wetted to cross",
            "Purple Pig only vulnerable when Matt grabs her"
        };
        chapter.requiresHose = true;
        chapter.hasVehicleCameo = true;
        chapter.vehicleDescription = "1996 burgundy GMC Jimmy";
        chapter.nextChapterId = "torch-lake";
        chapter.sceneName = "01_MeadowbrookPark";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_MeadowbrookPark.asset");
    }

    private void CreateTorchLake()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "torch-lake";
        chapter.title = "Torch Lake";
        chapter.location = "Cottage to Party Store";
        chapter.musicTag = "lake-theme";
        chapter.objectives = new string[] 
        {
            "Navigate from cottage to party store",
            "Explore lake environment",
            "Reach Monty Python homage ending"
        };
        chapter.hasBoss = false;
        chapter.availablePowerUps = new PowerUpType[] { PowerUpType.Hose };
        chapter.collectibleName = "Lake Stones";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Cottages + lake background theme",
            "End gag: Monty Python and Holy Grail homage"
        };
        chapter.nextChapterId = "notre-dame";
        chapter.sceneName = "02_TorchLake";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_TorchLake.asset");
    }

    private void CreateNotreDame()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "notre-dame";
        chapter.title = "Notre Dame";
        chapter.location = "Campus Traversal";
        chapter.musicTag = "campus-theme";
        chapter.objectives = new string[] 
        {
            "Traverse the campus",
            "Collect all 5 campus items",
            "Reach the exit"
        };
        chapter.hasBoss = false;
        chapter.collectibleName = "Campus Items";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Campus environment",
            "Focus on collectible gathering"
        };
        chapter.nextChapterId = "high-school";
        chapter.sceneName = "03_NotreDame";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_NotreDame.asset");
    }

    private void CreateHighSchool()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "high-school";
        chapter.title = "High School";
        chapter.location = "High School Campus";
        chapter.musicTag = "school-theme";
        chapter.objectives = new string[] 
        {
            "Navigate high school",
            "Find references to The King and I",
            "Locate jersey #88"
        };
        chapter.hasBoss = false;
        chapter.collectibleName = "School Memorabilia";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "References: The King and I, Once Upon a Mattress",
            "Jersey #88 appearance",
            "1989 Jeep Wrangler cameo"
        };
        chapter.hasVehicleCameo = true;
        chapter.vehicleDescription = "1989 Jeep Wrangler";
        chapter.nextChapterId = "philadelphia";
        chapter.sceneName = "04_HighSchool";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_HighSchool.asset");
    }

    private void CreatePhiladelphia()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "philadelphia";
        chapter.title = "Philadelphia";
        chapter.location = "City Streets";
        chapter.musicTag = "city-theme";
        chapter.objectives = new string[] 
        {
            "Defeat Hamburger Helper glove",
            "Collect all poker chips",
            "Navigate city environment"
        };
        chapter.hasBoss = true;
        chapter.bossType = "HamburgerHelperGlove";
        chapter.collectibleName = "Poker Chips";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Villain: Hamburger Helper glove",
            "Urban environment"
        };
        chapter.nextChapterId = "parsons-chicken";
        chapter.sceneName = "05_Philadelphia";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_Philadelphia.asset");
    }

    private void CreateParsonsChicken()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "parsons-chicken";
        chapter.title = "Parson's Chicken";
        chapter.location = "Restaurant to Airport";
        chapter.musicTag = "restaurant-theme";
        chapter.objectives = new string[] 
        {
            "Defeat obstructive staff",
            "Reach airport gate",
            "Prepare for travel to Costa Rica"
        };
        chapter.hasBoss = false; // Staff enemies, not a single boss
        chapter.collectibleName = "Restaurant Items";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Defeat staff to progress",
            "Gate to airport/travel system",
            "Preparation for international chapter"
        };
        chapter.nextChapterId = "costa-rica";
        chapter.sceneName = "06_ParsonsChicken";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_ParsonsChicken.asset");
    }

    private void CreateCostaRica()
    {
        var chapter = ScriptableObject.CreateInstance<ChapterData>();
        
        chapter.chapterId = "costa-rica";
        chapter.title = "Costa Rica";
        chapter.location = "Dominical → Casa Lumpusita";
        chapter.musicTag = "jungle-theme";
        chapter.objectives = new string[] 
        {
            "Navigate jungle/hill climb",
            "Survive jaguar and spider hazards",
            "Defeat Araña Reina (Spider Queen)",
            "Reach Casa Lumpusita",
            "Save Ellen at infinity pool"
        };
        chapter.hasBoss = true;
        chapter.bossType = "AranaReina";
        chapter.availablePowerUps = new PowerUpType[] { PowerUpType.Chiliguaro };
        chapter.collectibleName = "Jungle Artifacts";
        chapter.maxCollectibles = 5;
        chapter.specialRules = new string[] 
        {
            "Jungle/hill climb environment",
            "Hazards: jaguars, spiders",
            "Chiliguaro shot grants bouncing fireballs",
            "Final boss: Araña Reina",
            "Ending: family at infinity pool"
        };
        chapter.nextChapterId = ""; // Final chapter
        chapter.sceneName = "07_CostaRica";
        
        AssetDatabase.CreateAsset(chapter, "Assets/_Project/Scripts/ChapterData/Chapter_CostaRica.asset");
    }
}