using System.Collections.Generic;
using UnityEngine;

public class CutsceneNarratorBootstrap : MonoBehaviour
{
    [Header("References")]
    public CutsceneNarratorPlayer player;

    void Start()
    {
        if (!player)
        {
            player = FindObjectOfType<CutsceneNarratorPlayer>();
            if (!player) { Debug.LogError("CutsceneNarratorBootstrap: No player assigned/found."); return; }
        }

        player.slides = BuildSlides();
        player.Play();
    }

    List<CutsceneNarratorPlayer.Slide> BuildSlides()
    {
        // Ensure your PNGs exist in Assets/Resources/Cutscenes/ with these names:
        // I1_TenSuns_HouYi.png, I2_SunsAsSyndicates_City.png, I5_BlackVeil_Door.png,
        // I6_BossApproach_Den.png, O1_PistolFalls.png, O3_WhispersAlleys.png

        var list = new List<CutsceneNarratorPlayer.Slide>();

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "I1_TenSuns_HouYi",
            narration =
@"Long ago, the heavens themselves betrayed the earth. Ten suns rose together, scorching soil and ocean alike. Cities crumbled, forests withered, rivers boiled into steam.

But from the chaos came Hou Yi, the divine archer. With steady hand, he drew his bow, loosing arrows that split the sky. One by one, the suns fell, until only a single flame remained to guide mankind."
        });

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "I2_SunsAsSyndicates_City",
            narration =
@"High above the skyline, the heavens blaze. Nine suns flare brighter, their light searing through the storm, their gaze fixed upon you. One has fallen, but nine remain — and they are watching.

The city trembles with anticipation. Somewhere in the distance, a new sun rises, stronger, harsher, its fire already reaching. The path ahead is not victory. It is war."
        });

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "I5_BlackVeil_Door",
            narration =
@"Here, in the labyrinth of the slums, the first sun festers. The Black Veil Syndicate — smugglers, killers, merchants of poison."
        });

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "I6_BossApproach_Den",
            narration =
@"You don’t belong here, stranger. The boss is waiting… but you won’t leave breathing."
        });

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "O1_PistolFalls",
            narration =
@"A single echo breaks through the storm. The Kingpin’s pistol slips from his grasp, its gilded steel striking the neon-soaked concrete. For years it ruled these alleys, a symbol of fear, a mark of power.

Now it lies silent in the rain.

The Black Veil Syndicate crumbles with its master, their sun extinguished. But the city does not bow in silence. In the distance, fires still burn, and higher flames begin to stir."
        });

        list.Add(new CutsceneNarratorPlayer.Slide
        {
            spriteBaseName = "O3_WhispersAlleys",
            narration =
@"The slums do not sleep. The streets stir with murmurs carried by rain and static. Neon signs glitch with unseen signals, and shadows lean closer to listen.

Whispers crawl through the alleys like smoke. They speak of the fallen sun, of the Black Veil broken, of a name newly painted in blood. The words spread faster than fire, from the gutters to the towers."
        });

        return list;
    }
}
