using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.Data;
using HeroEditor.Common;
using UnityEngine;

namespace Assets.HeroEditor.Common.Scripts.Collections
{
	/// <summary>
	/// Stores firearms and their params.
	/// </summary>
	[CreateAssetMenu(fileName = "FirearmCollection", menuName = "HeroEditor/FirearmCollection")]
	public class FirearmCollection : ScriptableObject
	{
        public string Id;
		public List<FirearmParams> Firearms;

		public static Dictionary<string, FirearmCollection> Instances = new Dictionary<string, FirearmCollection>();

		// Guard to optionally skip auto-initialization in projects where Resources contain broken assets.
		public static bool SkipAutoInitialization = true;

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			if (SkipAutoInitialization)
			{
				// Intentionally skip loading to avoid noisy logs from broken resources.
				return;
			}
			Instances = Resources.LoadAll<FirearmCollection>("").ToDictionary(i => i.Id, i => i);
		}
		
		public void OnValidate()
        {
			var spriteCollection = FindObjectOfType<SpriteCollection>();

			if (spriteCollection == null) return;

			var entries = spriteCollection.Firearm1H.Union(spriteCollection.Firearm2H).ToList();

			foreach (var entry in entries)
			{
				if (Firearms.All(i => i.Name != entry.Name))
				{
					Debug.LogWarningFormat("Firearm params missed for: {0}", entry.Name);
				}
			}

			foreach (var p in Firearms)
			{
				if (entries.All(i => i.Name != p.Name))
				{
					Debug.LogWarningFormat("Excess params found: {0}", p.Name);
				}
			}

			foreach (var firearm in Firearms)
			{
				if (firearm.FirearmTexture != null)
				{
					firearm.Name = firearm.FirearmTexture.name;
				}
			}
		}

		public void UpdateNames()
		{
			foreach (var firearm in Firearms)
			{
				if (firearm.FirearmTexture == null)
				{
					Debug.LogWarningFormat("Please assign a texture for {0}", firearm.Name);
				}
				else
				{
					firearm.Name = firearm.FirearmTexture.name;
				}
			}
		}

		public void RemoveExcess()
		{
			var spriteCollection = FindObjectOfType<SpriteCollection>();
			var entries = spriteCollection.Firearm1H.Union(spriteCollection.Firearm2H).ToList();

			Firearms.RemoveAll(p => entries.All(i => i.Name != p.Name));
		}
	}
}
