using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TribeSim
{
    static class NamesGenerator
    {

        private static List<string> consonants = new List<string>() { "qu", "w", "r","t","p","s","d","f","g","h","j","k","l","z","x","c","v","b","n","m","nt","kl","gh","ck","rt","kh","nj","th","rs","st","bs","rk","sp","nd","dr","rd","ks"};
        private static List<string> vowels = new List<string>() { "e", "u", "i", "o", "a", "y", "ee", "ii", "ae", "oe", "ea","ei","eo","uu","ie","ue","aa","ao"};

        private static List<string> bodyparts = new List<string>() { "leg", "arm", "tooth", "eye", "head", "stomach", "soul", "back", "finger", "neck", "entire body", "knee", "toe", "thumb"};
        private static List<string> relatives = new List<string>() { "mother", "father", "brother", "sister", "son", "daughter", "stepsister", "wife", "mother in law", "grandmother", "god", "friend", "pet frog", "dog" };
        private static List<string> objects = new List<string>() { "spear", "boots", "will to live", "conciosness", "bow", "respect to others", "hunting socks", "lucky arrow", "waistband", "aim", "understanding of the reason of life", "mammoth bat", "sence of smell", "sence of direction" };
        private static List<string> otherExcuses = new List<string>() { "the weather is bad", "the forest is too scary", "he is a pacifist", "hunting is too old-fashioned", "he lost his hunting boots", "the gods are against it", "animals are too cute to kill", "he is not hungry", "he is too important for the tribe", "it's too dirty in the forest", "he prefers singing", "he doesn't like the weather", "he is afraid of mosquitoes", "there is too many spiders in the forest", "it is too gloomy outside", "he's not in the mood for hunting", "they are a bunch of loosers he doesn't want to hunt with" };

        private static List<string> uselessActions = new List<string>() { "to carve a bear in marble", "to jump up and down the grass", "to thankfully bang his head on a big tree", "to speak to the spirits of his ancestors", "to enchant the weather", "to heal his friend with a bunny tail and a feather", "to contemplate eternity", "to meditate", "to count the stars", "to sing to the sun", "to hide from the moon", "to make love potions", "to thank the rock for the successful hunt", "to dance", "to gather poisonous mushrooms", "to scatch his back", "to throw stones up and down", "to write poems about bears", "to look for solitude", "to pretend to be his own father", "to roar like thunder", "to pull his own hair to gain supernatural powers", "to strok his beard to look important", "to make a circle of fire around the village", "to draw circles", "to rythmically bang a hollow trunk", "to do nothing at all", "to build a shrine out of huge slabs of granite", "to search for shiny stones", "to trade usless stuff with friends" };
        private static List<string> TrickLikelyhoodActions = new List<string>() { "not to trust anyone", "to think of himself better than of the others", "to consider others not being worth it", "to overestimate his own importance", "to think that others won't survive without him", "to stop thinking that others are hungry", "to be an outright egoist", "not to care about others at all"};
        private static List<string> TrickEfficiencyActions = new List<string>() { "to pretend he is sick", "to pretend he is very hungry", "to snatch the food before anyone else", "to hide food on a tree", "to hide food in the ground", "to tell good pieces from the bad", "to hide the food in his hand", "to hide the food under a rock", "to ask others to look away", "to bang people on a head to eat in piece", "to befriend the leaders", "to earn respect of the tribe", "to flatter the leaders", "to behave like the elders", "to be liked by everyone", "to be the biggest and meanest himself", "to give advice to earn respect", "to mock others to earn status", "to care for his own status", "to induce fear in friends", "to lie" };
        private static List<string> TeachingLikelyhoodActions = new List<string>() { "to value the trust of the pupil", "to value the respect of the tribe", "to enjoy teaching", "to like living with educated people", "to like the sound of his own voice", "to get used to being a teacher", "to enjoy giving advice", "to value knowledge", "to want to pass the knowledge" };
        private static List<string> TeachingEfficiencyActions = new List<string>() { "to draw schemes with a stick", "to explain with colourful rocks", "to give homework", "to explain slowly", "not to bite a student", "to create funny mnemonic poems", "to sing together with his pupil", "to be patient", "to repeat many times", "to encourage his pupil", "not to study too much each day" };
        private static List<string> StudyLikelyhoodActions = new List<string>() { "to value knowledge", "to be curious", "to like to learn", "to be open for new ideas", "to be observant",  "to respect the wisdom", "to mimic other's behaviour", "to understand connection between knowledge and a better life", "to want to be like the best" };
        private static List<string> StudyEfficiencyActions = new List<string>() { "to make knots to remember", "to practice regularly", "to carve the notes on a tree bark", "to repeat things a few times", "to concentrate", "to use mnemonics to remember better", "to strive for perfection", "to revise regularly", "to respect the teacher", "not to run away from classes" };
        private static List<string> FreeRiderPunishmentLikelyhoodActions = new List<string>() { "not to believe in lame excuses",  "to hate lies", "to respect the teamwork", "to believe that everyone should contribute", "to believe that lazy people should remain hungry", "to value justice", "to disrespect laziness", "not to befriend bad hunters"};
        private static List<string> FreeRiderDeterminationEfficiencyActions = new List<string>() { "to be a good judge of character", "to judge by the actions, not the words", "to count the hunters", "to tell real friends", "to tell lies from truths", "to remember other's actions", "not to trust the liars", "to watch the eyes" };
        private static List<string> LikelyhoodOfNotBeingAFreeRiderActions = new List<string>() { "to be honest at all times", "to wish to contribute", "to be responsible", "to be a team player", "to like to hunt with others", "to like to help others", "to like to hunt", "to value the needs of others above his own", "to consider laziness bad" };
        private static List<string> HuntingEfficiencyActions = new List<string>() { "to throw stones", "to throw sticks", "to make ambushes", "to dig holes", "to wash himself before the hunt", "to be quiet", "to sit on trees", "to throw heavy objects", "to throw stuff from the trees", "to jump from the trees", "to scare animals with fire", "to scare animals with cries", "to approach animals from upwind", "to make pointed sticks", "to aim better", "to bite harder", "to hit the weakspot", "to aim for the eyes", "to grab the tail", "to run fast", "to climb high", "to swim fast", "to jump quietly", "to mislead the prey", "to kill with one throw", "to make landslides", "to read the footprints" };
        private static List<string> CooperationEfficiencyActions = new List<string>() { "to use handsigns", "to watch others", "to make plans", "to follow plans", "to use footsigns", "to use body language", "to make meaningful sounds", "to listen to each other", "to think as a group", "to use broken twigs as signs for each other" };
        
        private static Random random = new Random();
        private static object _lock = new object();

        public static string GenerateName()
        {
            StringBuilder  sb = new StringBuilder();
            
            lock (_lock) {
                sb.AppendFormat("{0}{1}", consonants[random.Next(consonants.Count)], vowels[random.Next(vowels.Count)]);
                if (random.Flip())
                {
                    sb.Append(consonants[random.Next(consonants.Count)]);
                }
                sb.AppendFormat("-{0}{1}", UppercaseFirst(consonants[random.Next(consonants.Count)]), vowels[random.Next(vowels.Count)]);
                if (random.Flip())
                {
                    sb.Append(consonants[random.Next(consonants.Count)]);
                }                
            }                                    
            return UppercaseFirst(sb.ToString());
        }

        public static string GenerateTribeName()
        {
            StringBuilder sb = new StringBuilder();

            lock (_lock) {
                if (random.Flip())
                {
                    sb.Append(vowels[random.Next(vowels.Count)]);
                }
                sb.AppendFormat("{0}{1}", consonants[random.Next(consonants.Count)], vowels[random.Next(vowels.Count)]);
                if (random.Flip())
                {
                    sb.Append(consonants[random.Next(consonants.Count)]);
                }

                if (sb.ToString().ToLower() == "con") return GenerateTribeName();
            }
            return UppercaseFirst(sb.ToString());
        }

        public static string GenerateLameExcuse()
        {
            string format;
            string variable;
            lock (_lock)
                switch (random.Next(4))
                {
                    case 0:
                        format = "his {0} hurts";
                        variable = bodyparts[random.Next(bodyparts.Count)];
                        break;
                    case 1:
                        format = "his {0} is sick";
                        variable = relatives[random.Next(relatives.Count)];
                        break;
                    case 2:
                        format = "he lost his {0}";
                        variable = objects[random.Next(objects.Count)];
                        break;
                    default:
                        format = "{0}";
                        variable = otherExcuses[random.Next(otherExcuses.Count)];
                        break;
                }
            return string.Format(format, variable);
        }

        public static string GenerateBodypart()
        {
            lock (_lock)
                return bodyparts[random.Next(bodyparts.Count)];
        }

        public static string GenerateActionDescription(AvailableFeatures af, bool negate = false)
        {
            List<string> options = null;
            switch (af)
            {
                case AvailableFeatures.CooperationEfficiency:
                    options = CooperationEfficiencyActions;
                    break;
                case AvailableFeatures.FreeRiderDeterminationEfficiency:
                    options = FreeRiderDeterminationEfficiencyActions;
                    break;
                case AvailableFeatures.FreeRiderPunishmentLikelyhood:
                    options = FreeRiderPunishmentLikelyhoodActions;
                    break;
                case AvailableFeatures.HuntingEfficiency:
                    options = HuntingEfficiencyActions;
                    break;
                case AvailableFeatures.LikelyhoodOfNotBeingAFreeRider:
                    options = LikelyhoodOfNotBeingAFreeRiderActions;
                    break;
                case AvailableFeatures.StudyEfficiency:
                    options = StudyEfficiencyActions;
                    break;
                case AvailableFeatures.StudyLikelyhood:
                    options = StudyLikelyhoodActions;
                    break;
                case AvailableFeatures.TeachingEfficiency:
                    options = TeachingEfficiencyActions;
                    break;
                case AvailableFeatures.TeachingLikelyhood:
                    options = TeachingLikelyhoodActions;
                    break;
                case AvailableFeatures.TrickEfficiency:
                    options = TrickEfficiencyActions;
                    break;
                case AvailableFeatures.TrickLikelyhood:
                    options = TrickLikelyhoodActions;
                    break;  
                case AvailableFeatures.UselessActionsLikelihood:
                    options = uselessActions;
                    break;
            }

            if (options == null) return "";

            string retval = null;
            lock (_lock)
                retval = options[random.Next(options.Count)];

            if (negate)
            {
                if (retval.IndexOf("not")>-1)
                {
                    retval = retval.Replace("not ", "");
                }
                else
                {
                    Regex regex = new Regex(Regex.Escape("to"));
                    retval = regex.Replace(retval, "not to", 1);
                }
            }

            return retval;
        }

        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static bool Flip() {
            lock (_lock)
                return random.Flip();
        }
    }
}
