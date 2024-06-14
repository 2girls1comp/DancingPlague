using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows.Forms;


namespace DancingPlague
{
   
    public class DancingPlague : Script
    {
        float dist = 1000f;
        Ped[] NearbyPeds = World.GetNearbyPeds(Game.Player.Character, 100f);
        List<string> AnimDict = new List<string>();
        List<string> AnimName = new List<string>();

        List<Model> NPCModel= new List<Model>();
        List<Ped> dancingNPCs = new List<Ped>();

        RelationshipGroup PLAYER_LOVE = World.AddRelationshipGroup("PLAYER_LOVE"); //we "mark" the dancing NPC with this relationship int
        RelationshipGroup PED_LOVE = World.AddRelationshipGroup("PED_LOVE"); //we "mark" the dancing NPC with this relationship int

        bool playing = false;
        SoundPlayer soundPlayer = new SoundPlayer("./scripts/dancingPlague/dancing_plague.wav"); //audio player
        

        string RadioStation = "OFF";
        public DancingPlague()
        {
            //DANCE ANIMATIONS
            //lap dance
            AnimDict.Add("switch@trevor@mocks_lapdance");
            AnimName.Add("001443_01_trvs_28_idle_stripper");
            //private dance pt1
            AnimDict.Add("mini@strip_club@private_dance@part1");
            AnimName.Add("priv_dance_p1");
            //private dance pt2
            AnimDict.Add("mini@strip_club@private_dance@part2");
            AnimName.Add("priv_dance_p2");
            //private dance pt3
            AnimDict.Add("mini@strip_club@private_dance@part3");
            AnimName.Add("priv_dance_p3");
            //dance crowd
            AnimDict.Add("anim@amb@nightclub@lazlow@hi_dancefloor@");
            AnimName.Add("dancecrowd_li_15_handup_laz");
            //dance face dj
            AnimDict.Add("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity");
            AnimName.Add("hi_dance_facedj_09_v2_female^1");
            //dance loop tao
            AnimDict.Add("misschinese2_crystalmazemcs1_cs");
            AnimName.Add("dance_loop_tao");
            //dance crowd 2
            AnimDict.Add("anim@amb@nightclub@lazlow@hi_dancefloor@");
            AnimName.Add("dancecrowd_li_11_hu_shimmy_laz");
            //lap dance girl
            AnimDict.Add("mp_safehouse");
            AnimName.Add("lap_dance_girl");
            //lap dance 2
            AnimDict.Add("anim@amb@nightclub@peds@");
            AnimName.Add("mini_strip_club_lap_dance_ld_girl_a_song_a_p1");
            //private dance idle
            AnimDict.Add("anim@amb@nightclub@peds@");
            AnimName.Add("mini_strip_club_private_dance_idle_priv_dance_idle");
            //sexy grind idle
            AnimDict.Add("anim@amb@nightclub@lazlow@hi_podium@");
            AnimName.Add("danceidle_li_15_sexygrind_laz");
            //private dance pt2
            AnimDict.Add("mini@strip_club@private_dance@part2");
            AnimName.Add("priv_dance_p2");

            this.Tick += onTick;
            this.KeyUp += onKeyUp;
            this.KeyDown += onKeyDown;
        }

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom)
            {
                return getrandom.Next(min, max);
            }
        }
        private void onTick(object sender, EventArgs e) //this function gets executed continuously 
        {
            
            if (playing)
            {
                //random seed for the dance animation
                Random rnd = new Random();
                Random rndT = new Random();
                Random rndS = new Random();

                if (Game.Player.Character.IsInVehicle() == true)
                {
                    Function.Call(Hash.SET_RADIO_TO_STATION_NAME, "OFF");
                }

                //Set Group Relationshiop between character and peds
                Game.Player.Character.RelationshipGroup = PLAYER_LOVE;
                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, Relationship.Companion, PLAYER_LOVE, PED_LOVE);
                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, Relationship.Companion, PED_LOVE, PLAYER_LOVE);
                Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player.Character, true);

                NearbyPeds = World.GetNearbyPeds(Game.Player.Character, dist);
                if (NearbyPeds.Count() > 0)
                {
                    for (int i = 0; i < NearbyPeds.Count(); i++)
                    {
                        if ((Function.Call<bool>(Hash.IS_PED_MALE, NearbyPeds[i]) == true) && (NearbyPeds[i].IsHuman == true))
                        {
                            //configure the ped so that they don't get scared and start a run away animation
                            Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 117, false); //CPED_CONFIG_FLAG_BumpedByPlayer = 117,
                            Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 128, false); //CPED_CONFIG_FLAG_CanBeAgitated = 128,
                            Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 183, false); //CPED_CONFIG_FLAG_IsAgitated = 183,

                            /* Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 148, false); //CPED_CONFIG_FLAG_CanAttackFriendly = 140,

                             Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 225, false); //CPED_CONFIG_FLAG_DisablePotentialToBeWalkedIntoResponse = 225,
                             Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 226, false); //CPED_CONFIG_FLAG_DisablePedAvoidance = 226,
                             Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 294, false);//CPED_CONFIG_FLAG_DisableShockingEvents = 294,
                             Function.Call(Hash.SET_PED_CONFIG_FLAG, NearbyPeds[i], 329, false); //CPED_CONFIG_FLAG_DisableTalkTo = 329,
                            */
                            //Function.Call(Hash.DISABLE_PED_PAIN_AUDIO, true);
                            //Function.Call(Hash.DISABLE_PED_INJURED_ON_GROUND_BEHAVIOUR, true)

                            NearbyPeds[i].RelationshipGroup = PED_LOVE;

                            
                            //if they are not having a task animation
                            if ((Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, NearbyPeds[i], 134) == false) && (NearbyPeds[i].IsAlive)) //in TaskIndex 134 is CTaskScriptedAnimation which seems to work https://alloc8or.re/gta5/doc/enums/eTaskTypeIndex.txt
                            {
                                //Stop current animations and tasks
                                NearbyPeds[i].Task.ClearAllImmediately();
                                NearbyPeds[i].IsInvincible = true;
                                //get a random dance animation
                                int randomDance = GetRandomNumber(0, AnimName.Count());
                                //load animation dictionary
                                Function.Call(Hash.REQUEST_ANIM_DICT, AnimDict[randomDance]);
                                while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, AnimDict[randomDance])) Wait(10);


                                //Dance
                                Function.Call(Hash.TASK_PLAY_ANIM, NearbyPeds[i], AnimDict[randomDance], AnimName[randomDance], 8.0, 8.0 * -1, -1, 1, 0, false, false, false);
                                //Never stop dancing!
                                Function.Call(Hash.SET_PED_KEEP_TASK, NearbyPeds[i], true);


                                float randomDanceTime = rndT.Next(0, 100);
                                float randomDanceSpeed = rndS.Next(0, 100);

                                Function.Call(Hash.SET_ENTITY_ANIM_CURRENT_TIME, NearbyPeds[i], AnimDict[randomDance], AnimName[randomDance], randomDanceTime * 0.01f);

                                Function.Call(Hash.SET_ENTITY_ANIM_SPEED, NearbyPeds[i], AnimDict[randomDance], AnimName[randomDance], randomDanceSpeed * 0.01f);

                                //GTA.UI.Screen.ShowSubtitle("Anim play time: " + (Function.Call<double>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, NearbyPeds[i], AnimDict[randomDance], AnimName[randomDance])));
                                

                                dancingNPCs.Add(NearbyPeds[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                GTA.UI.Screen.ShowHelpText("~w~ Press H to start Dancing Plague");

                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, Relationship.Neutral, PLAYER_LOVE, PED_LOVE);
                Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, Relationship.Neutral, PED_LOVE, PLAYER_LOVE);

                if (dancingNPCs.Count() > 0)
                {
                    for(int i = 0; i < dancingNPCs.Count(); i++)
                    {
                        if (dancingNPCs[i].IsAlive)
                        {
                            dancingNPCs[i].IsInvincible = false;
                            dancingNPCs[i].Task.ClearAllImmediately();
                        }
                    }
                    dancingNPCs.Clear();
                }

            }
            
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H)
            {
                if (playing)
                {
                    if (Game.Player.Character.IsInVehicle() == true)
                    {
                        Function.Call(Hash.SET_RADIO_TO_STATION_NAME, RadioStation);
                    }
                    soundPlayer.Stop(); //start playback of the .wav file}
                }
                playing= false;
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H)
            {
                if(!playing)
                {
                    if (Game.Player.Character.IsInVehicle() == true)
                    {
                        RadioStation = Function.Call<string>(Hash.GET_PLAYER_RADIO_STATION_NAME);
                    }
                    soundPlayer.PlayLooping(); //start playback of the .wav file
                }
                playing = true; 
            }
        }
    }
}
