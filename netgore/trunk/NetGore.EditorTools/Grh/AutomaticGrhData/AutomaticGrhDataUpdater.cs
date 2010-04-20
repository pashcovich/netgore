using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using NetGore.Content;
using NetGore.Graphics;
using NetGore.IO;
using SFML;
using SFML.Graphics;

namespace NetGore.EditorTools
{
    /// <summary>
    /// Handles the automatic GrhData generation.
    /// </summary>
    public static class AutomaticGrhDataUpdater
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static readonly string[] _graphicFileSuffixes = {
            ".bmp", ".jpg", ".jpeg", ".dds", ".psd", ".png", ".gif", ".tga", ".hdr"
        };

        /// <summary>
        /// Finds all of the texture files from the root directory.
        /// </summary>
        /// <param name="rootDir">Root directory to search from.</param>
        /// <returns>List of the complete file paths from the <paramref name="rootDir"/>.</returns>
        static List<string> FindTextures(string rootDir)
        {
            var ret = new List<string>();
            var dirs = GetStationaryDirectories(rootDir);

            foreach (var dir in dirs)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    var f = file;
                    if (!_graphicFileSuffixes.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    ret.Add(f.Replace('\\', '/'));
                }
            }

            return ret;
        }

        /// <summary>
        /// Finds all of the used textures.
        /// </summary>
        /// <returns>Dictionary where the key is the virtual texture name (relative path minus the file extension) and
        /// the value is a list of GrhDatas that use that texture.</returns>
        static Dictionary<string, List<GrhData>> FindUsedTextures()
        {
            var ret = new Dictionary<string, List<GrhData>>(StringComparer.OrdinalIgnoreCase);

            // Loop through every stationary GrhData
            foreach (var gd in GrhInfo.GrhDatas.OfType<StationaryGrhData>())
            {
                var textureName = gd.TextureName.ToString();
                List<GrhData> dictList;

                // Get the existing list, or create a new one if the first entry
                if (!ret.TryGetValue(textureName, out dictList))
                {
                    dictList = new List<GrhData>();
                    ret.Add(textureName, dictList);
                }

                // Add the GrhData to the list
                dictList.Add(gd);
            }

            return ret;
        }

        /// <summary>
        /// Gets the directories that are for automatic animations.
        /// </summary>
        /// <param name="rootDir">The root directory.</param>
        /// <returns>The directories that contain textures and are for automatic animations.</returns>
        static IEnumerable<string> GetAnimatedDirectories(string rootDir)
        {
            if (AutomaticAnimatedGrhData.IsAutomaticAnimatedGrhDataDirectory(rootDir))
                yield return rootDir;
            else
            {
                foreach (var dir in Directory.GetDirectories(rootDir))
                {
                    foreach (var dir2 in GetAnimatedDirectories(dir))
                    {
                        yield return dir2;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the amount to trim off of a directory to make it relative to the specified <paramref name="rootDir"/>.
        /// </summary>
        /// <param name="rootDir">Root directory to make other directories relative to.</param>
        /// <returns>The amount to trim off of a directory to make it relative to the specified
        /// <paramref name="rootDir"/>.</returns>
        static int GetRelativeTrimLength(string rootDir)
        {
            var len = rootDir.Length;
            if (!rootDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                len++;
            return len;
        }

        /// <summary>
        /// Gets the directories that are not for automatic animations.
        /// </summary>
        /// <param name="rootDir">The root directory.</param>
        /// <returns>The directories that contain textures that are not for automatic animations.</returns>
        static IEnumerable<string> GetStationaryDirectories(string rootDir)
        {
            yield return rootDir;

            foreach (var dir in Directory.GetDirectories(rootDir))
            {
                if (AutomaticAnimatedGrhData.IsAutomaticAnimatedGrhDataDirectory(dir))
                    continue;

                foreach (var dir2 in GetStationaryDirectories(dir))
                {
                    yield return dir2;
                }
            }
        }

        /// <summary>
        /// Gets the size of a texture.
        /// </summary>
        /// <param name="cm">The <see cref="IContentManager"/> to use to load the asset.</param>
        /// <param name="filePath">Absolute file path to the texture.</param>
        /// <returns>Size of the texture.</returns>
        static Vector2 GetTextureSize(IContentManager cm, string filePath)
        {
            var img = cm.LoadImage(filePath, ContentLevel.Temporary);
            return new Vector2(img.Width, img.Height);
        }

        /// <summary>
        /// Converts an absolute path to a virtual texture path (relative path minus the extension).
        /// </summary>
        /// <param name="trimLen">Amount to trim off to make the path relative.</param>
        /// <param name="absolute">Absolute path to find the relative path of.</param>
        /// <returns>The relative path of the specified absolute path.</returns>
        static string TextureAbsoluteToRelativePath(int trimLen, string absolute)
        {
            absolute = absolute.Replace('\\', '/');

            // Trim down to the relative path
            var rel = absolute.Substring(trimLen);

            // Remove the file suffix since we don't use that
            var lastPeriod = rel.LastIndexOf('.');
            rel = rel.Substring(0, lastPeriod);

            return rel;
        }

        /// <summary>
        /// Updates all of the automaticly added GrhDatas.
        /// </summary>
        /// <param name="cm"><see cref="IContentManager"/> to use for new GrhDatas.</param>
        /// <param name="rootGrhDir">Root Grh texture directory.</param>
        /// <returns>IEnumerable of all of the new GrhDatas created.</returns>
        public static IEnumerable<GrhData> UpdateAll(IContentManager cm, string rootGrhDir)
        {
            var created = UpdateStationary(cm, rootGrhDir).Concat(UpdateAnimated(cm, rootGrhDir));

            if (log.IsInfoEnabled)
                log.WarnFormat("Automatic GrhData creation update resulted in `{0}` new GrhData(s).", created.Count());

            if (log.IsDebugEnabled && created.Count() > 0)
            {
                log.Debug("The following GrhDatas were created:");
                foreach (var c in created)
                {
                    log.Debug(" * " + c);
                }
            }

            return created;
        }

        /// <summary>
        /// Updates the animated automaticly added GrhDatas.
        /// </summary>
        /// <param name="cm"><see cref="IContentManager"/> to use for new GrhDatas.</param>
        /// <param name="rootGrhDir">Root Grh texture directory.</param>
        /// <returns>IEnumerable of all of the new GrhDatas created.</returns>
        public static IEnumerable<GrhData> UpdateAnimated(IContentManager cm, string rootGrhDir)
        {
            if (log.IsInfoEnabled)
                log.InfoFormat("Searching for automatic animated GrhDatas from root `{0}`.");

            var ret = new List<GrhData>();

            // Find all directories that match the needed pattern
            var dirs = GetAnimatedDirectories(rootGrhDir);

            foreach (var dir in dirs)
            {
                // Grab the animation info from the directory
                var animInfo = AutomaticAnimatedGrhData.GetAutomaticAnimationInfo(dir);
                if (animInfo == null)
                    continue;

                // Get the virtual directory (remove the root)
                var partialDir = dir.Substring(rootGrhDir.Length);
                if (partialDir.StartsWith(Path.DirectorySeparatorChar.ToString()) ||
                    partialDir.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                    partialDir = partialDir.Substring(1);

                // Get the categorization
                var lastDirSep = partialDir.LastIndexOf(Path.DirectorySeparatorChar);
                if (lastDirSep < 0)
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("Animated GrhData found at `{0}`, but could not be created because it has no category.");
                    continue;
                }

                var categoryStr = partialDir.Substring(0, lastDirSep);

                var categorization = new SpriteCategorization(new SpriteCategory(categoryStr), new SpriteTitle(animInfo.Title));

                // Create the AutomaticAnimatedGrhData
                var gd = GrhInfo.CreateAutomaticAnimatedGrhData(cm, categorization);
                if (gd != null)
                    ret.Add(gd);

                if (log.IsInfoEnabled)
                    log.InfoFormat("Automatic creation of animated GrhData `{0}`.", gd);
            }

            return ret;
        }

        /// <summary>
        /// Updates the stationary automaticly added GrhDatas.
        /// </summary>
        /// <param name="cm"><see cref="IContentManager"/> to use for new GrhDatas.</param>
        /// <param name="rootGrhDir">Root Grh texture directory.</param>
        /// <returns>IEnumerable of all of the new GrhDatas created.</returns>
        public static IEnumerable<GrhData> UpdateStationary(IContentManager cm, string rootGrhDir)
        {
            var dirSepChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

            if (log.IsInfoEnabled)
                log.InfoFormat("Searching for automatic stationary GrhDatas from root `{0}`.");

            // Get a List of all of the textures from the root directory
            var textures = FindTextures(rootGrhDir);

            // Get a List of all of the used textures
            var usedTextures = FindUsedTextures();

            // Grab the relative path instead of the complete file path since this
            // is how they are stored in the GrhData, then if it is in the usedTextures, remove it
            var trimLen = GetRelativeTrimLength(rootGrhDir);
            textures.RemoveAll(x => usedTextures.ContainsKey(TextureAbsoluteToRelativePath(trimLen, x)));

            // Check if there are any unused textures
            if (textures.Count == 0)
                return Enumerable.Empty<GrhData>();

            // Create the GrhDatas
            var ret = new List<GrhData>();
            foreach (var texture in textures)
            {
                // Go back to the relative path, and use it to figure out the categorization
                var relative = TextureAbsoluteToRelativePath(trimLen, texture);
                if (relative.LastIndexOfAny(dirSepChars) < 0)
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("Stationary GrhData found at `{0}`, but could not be created because it has no category.");
                    continue;
                }

                var categorization = SpriteCategorization.SplitCategoryAndTitle(relative);

                // Ensure the GrhData doesn't already exist
                if (GrhInfo.GetData(categorization) != null)
                    continue;

                // Read the texture size from the file
                Vector2 size;
                try
                {
                    size = GetTextureSize(cm, texture);
                }
                catch (LoadingFailedException ex)
                {
                    const string errmsg = "Failed to load asset from file `{0}` when trying to acquire the size: {1}";
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, texture, ex);
                    Debug.Fail(string.Format(errmsg, texture, ex));
                    continue;
                }

                // Create the GrhData
                var gd = GrhInfo.CreateGrhData(cm, categorization, relative, Vector2.Zero, size);
                gd.AutomaticSize = true;
                ret.Add(gd);

                if (log.IsInfoEnabled)
                    log.InfoFormat("Automatic creation of stationary GrhData `{0}`.", gd);
            }

            // Clear the temporary content
            cm.Unload(ContentLevel.Temporary);

            return ret;
        }
    }
}