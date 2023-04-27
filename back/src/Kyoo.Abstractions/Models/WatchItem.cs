// Kyoo - A portable and vast media library solution.
// Copyright (c) Kyoo.
//
// See AUTHORS.md and LICENSE file in the project root for full license information.
//
// Kyoo is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// Kyoo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Kyoo. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kyoo.Abstractions.Controllers;
using Kyoo.Abstractions.Models.Attributes;
using PathIO = System.IO.Path;

namespace Kyoo.Abstractions.Models
{
	/// <summary>
	/// A watch item give information useful for playback.
	/// Information about tracks and display information that could be used by the player.
	/// This contains mostly data from an <see cref="Episode"/> with another form.
	/// </summary>
	public class WatchItem : CustomTypeDescriptor, IThumbnails, ILink
	{
		/// <summary>
		/// The ID of the episode associated with this item.
		/// </summary>
		public int EpisodeID { get; set; }

		/// <summary>
		/// The slug of this episode.
		/// </summary>
		public string Slug { get; set; }

		/// <summary>
		/// The title of the show containing this episode.
		/// </summary>
		public string ShowTitle { get; set; }

		/// <summary>
		/// The slug of the show containing this episode
		/// </summary>
		public string ShowSlug { get; set; }

		/// <summary>
		/// The season in witch this episode is in.
		/// </summary>
		public int? SeasonNumber { get; set; }

		/// <summary>
		/// The number of this episode is it's season.
		/// </summary>
		public int? EpisodeNumber { get; set; }

		/// <summary>
		/// The absolute number of this episode. It's an episode number that is not reset to 1 after a new season.
		/// </summary>
		public int? AbsoluteNumber { get; set; }

		/// <summary>
		/// The title of this episode.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The summary of this episode.
		/// </summary>
		public string Overview { get; set; }

		/// <summary>
		/// The release date of this episode. It can be null if unknown.
		/// </summary>
		public DateTime? ReleaseDate { get; set; }

		/// <summary>
		/// The path of the video file for this episode. Any format supported by a <see cref="IFileSystem"/> is allowed.
		/// </summary>
		[SerializeIgnore] public string Path { get; set; }

		/// <summary>
		/// The episode that come before this one if you follow usual watch orders.
		/// If this is the first episode or this is a movie, it will be null.
		/// </summary>
		public Episode PreviousEpisode { get; set; }

		/// <summary>
		/// The episode that come after this one if you follow usual watch orders.
		/// If this is the last aired episode or this is a movie, it will be null.
		/// </summary>
		public Episode NextEpisode { get; set; }

		/// <summary>
		/// <c>true</c> if this is a movie, <c>false</c> otherwise.
		/// </summary>
		public bool IsMovie { get; set; }

		/// <inheritdoc />
		public Dictionary<int, string> Images { get; set; }

		/// <summary>
		/// The container of the video file of this episode.
		/// Common containers are mp4, mkv, avi and so on.
		/// </summary>
		public string Container { get; set; }

		/// <summary>
		/// The video track. See <see cref="Track"/> for more information.
		/// </summary>
		public Track Video { get; set; }

		/// <summary>
		/// The list of audio tracks. See <see cref="Track"/> for more information.
		/// </summary>
		public ICollection<Track> Audios { get; set; }

		/// <summary>
		/// The list of subtitles tracks. See <see cref="Track"/> for more information.
		/// </summary>
		public ICollection<Track> Subtitles { get; set; }

		/// <summary>
		/// The list of fonts that can be used to draw the subtitles.
		/// </summary>
		public ICollection<Font> Fonts { get; set; }

		/// <summary>
		/// The list of chapters. See <see cref="Chapter"/> for more information.
		/// </summary>
		public ICollection<Chapter> Chapters { get; set; }

		string _Type => IsMovie ? "movie" : "episode";

		/// <inheritdoc/>
		public object Link => new[]
		{
			new { Name = "Pristine", Link = $"/video/{_Type}/{Slug}/direct", Type = "direct" },
			new { Name = "Original", Link = $"/video/{_Type}/{Slug}/original/index.m3u8", Type = "transmux" },
			new { Name = "Auto", Link = $"/video/{_Type}/{Slug}/auto/index.m3u8", Type = "transcode-auto" },

			new { Name = "8K", Link = $"/video/{_Type}/{Slug}/8k/index.m3u8", Type = "transcode", },
			new { Name = "4K", Link = $"/video/{_Type}/{Slug}/4k/index.m3u8", Type = "transcode" },
			new { Name = "1440p", Link = $"/video/{_Type}/{Slug}/1440p/index.m3u8", Type = "transcode" },
			new { Name = "1080p", Link = $"/video/{_Type}/{Slug}/1080p/index.m3u8", Type = "transcode" },
			new { Name = "720p", Link = $"/video/{_Type}/{Slug}/720p/index.m3u8", Type = "transcode" },
			new { Name = "480p", Link = $"/video/{_Type}/{Slug}/480p/index.m3u8", Type = "transcode" },
			new { Name = "360p", Link = $"/video/{_Type}/{Slug}/360p/index.m3u8", Type = "transcode" },
			new { Name = "240p", Link = $"/video/{_Type}/{Slug}/240p/index.m3u8", Type = "transcode" },
		};

		/// <summary>
		/// Create a <see cref="WatchItem"/> from an <see cref="Episode"/>.
		/// </summary>
		/// <param name="ep">The episode to transform.</param>
		/// <param name="library">
		/// A library manager to retrieve the next and previous episode and load the show and tracks of the episode.
		/// </param>
		/// <param name="fs">A file system used to retrieve chapters informations.</param>
		/// <param name="transcoder">The transcoder used to list fonts.</param>
		/// <returns>A new WatchItem representing the given episode.</returns>
		public static async Task<WatchItem> FromEpisode(Episode ep, ILibraryManager library, IFileSystem fs, ITranscoder transcoder)
		{
			await library.Load(ep, x => x.Show);
			await library.Load(ep, x => x.Tracks);

			return new WatchItem
			{
				EpisodeID = ep.ID,
				Slug = ep.Slug,
				ShowSlug = ep.Show.Slug,
				ShowTitle = ep.Show.Title,
				SeasonNumber = ep.SeasonNumber,
				EpisodeNumber = ep.EpisodeNumber,
				AbsoluteNumber = ep.AbsoluteNumber,
				Title = ep.Title,
				Overview = ep.Overview,
				ReleaseDate = ep.ReleaseDate,
				Path = ep.Path,
				Images = ep.Show.Images,
				Container = PathIO.GetExtension(ep.Path).Replace(".", string.Empty),
				Video = ep.Tracks.FirstOrDefault(x => x.Type == StreamType.Video),
				Audios = ep.Tracks.Where(x => x.Type == StreamType.Audio).ToArray(),
				Subtitles = ep.Tracks.Where(x => x.Type == StreamType.Subtitle).ToArray(),
				Fonts = await transcoder.ListFonts(ep),
				PreviousEpisode = ep.Show.IsMovie
					? null
					: (await library.GetAll<Episode>(
							where: x => x.ShowID == ep.ShowID,
							limit: new Pagination(1, ep.ID, true)
						)).FirstOrDefault(),
				NextEpisode = ep.Show.IsMovie
					? null
					: (await library.GetAll<Episode>(
							where: x => x.ShowID == ep.ShowID,
							limit: new Pagination(1, ep.ID)
						)).FirstOrDefault(),
				Chapters = await _GetChapters(ep, fs),
				IsMovie = ep.Show.IsMovie
			};
		}

		// TODO move this method in a controller to support abstraction.
		private static async Task<ICollection<Chapter>> _GetChapters(Episode episode, IFileSystem fs)
		{
			string path = fs.Combine(
				await fs.GetExtraDirectory(episode),
				"Chapters",
				PathIO.GetFileNameWithoutExtension(episode.Path) + ".txt"
			);
			if (!await fs.Exists(path))
				return Array.Empty<Chapter>();
			try
			{
				using StreamReader sr = new(await fs.GetReader(path));
				string chapters = await sr.ReadToEndAsync();
				return chapters.Split('\n')
					.Select(x =>
					{
						string[] values = x.Split(' ');
						if (
							values.Length < 3
							|| !float.TryParse(values[0], out float start)
							|| !float.TryParse(values[1], out float end)
						)
							return null;
						return new Chapter(
							start,
							end,
							string.Join(' ', values.Skip(2))
						);
					})
					.Where(x => x != null)
					.ToArray();
			}
			catch (Exception ex)
			{
				await Console.Error.WriteLineAsync($"Invalid chapter file at {path}");
				Console.Error.WriteLine(ex.ToString());
				return Array.Empty<Chapter>();
			}
		}

		/// <inheritdoc />
		public override string GetClassName()
		{
			return nameof(Show);
		}

		/// <inheritdoc />
		public override string GetComponentName()
		{
			return ShowSlug;
		}
	}
}
