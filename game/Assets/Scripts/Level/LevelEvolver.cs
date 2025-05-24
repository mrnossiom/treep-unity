using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Treep.Level {
    public static class IListExtensions {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        private static void Shuffle<T>(this IList<T> ts, Random rng) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = rng.Next(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
        }

        /// <summary>
        /// Returns element in a random order based on given RNG.
        /// </summary>
        public static IEnumerable<T> RandomOrderAccess<T>(this IList<T> list, Random rng) {
            var values = Enumerable.Range(0, list.Count).ToArray();
            values.Shuffle(rng);

            foreach (var index in values) {
                yield return list[index];
            }
        }
    }

    public class PlacedRoom {
        public RoomData Template { get; private set; }
        public Vector2 Position { get; private set; }
        public Rect Area => new(this.Position, this.Template.size);

        public PlacedRoom(RoomData template, Vector2 position) {
            this.Template = template;
            this.Position = position;
        }

        public bool DoOverlap(PlacedRoom lastPlacedRoom) {
            var selfRect = this.Template.size;
            var otherRect = lastPlacedRoom.Template.size;

            return this.Position.x < lastPlacedRoom.Position.x + otherRect.x
                   && this.Position.x + selfRect.x > lastPlacedRoom.Position.x
                   && this.Position.y < lastPlacedRoom.Position.y + otherRect.y
                   && this.Position.y + selfRect.y > lastPlacedRoom.Position.y;
        }
    }

    public class LevelEvolver {
        private readonly List<RoomKind> _blueprint;
        private readonly Dictionary<RoomKind, List<RoomData>> _roomProviders;

        private List<PlacedRoom> _placedRooms = new();
        private readonly List<Vector2> _enemySpawners = new();
        private readonly List<Vector2> _spawnPoints = new();
        private readonly List<Vector2> _exitPoints = new();

        public Bounds Bounds { get; private set; }

        public ReadOnlyCollection<PlacedRoom> PlacedRooms => this._placedRooms.AsReadOnly();
        public ReadOnlyCollection<Vector2> EnemySpawners => this._enemySpawners.AsReadOnly();
        public ReadOnlyCollection<Vector2> SpawnPoints => this._spawnPoints.AsReadOnly();
        public ReadOnlyCollection<Vector2> ExitPoints => this._exitPoints.AsReadOnly();

        public LevelEvolver(List<RoomKind> blueprint, Dictionary<RoomKind, List<RoomData>> roomProviders) {
            this._blueprint = blueprint;
            this._roomProviders = roomProviders;
        }

        public bool EvolveRoot(Random rng) {
            var found = false;

            var evolved = new List<PlacedRoom>();
            const int rootId = 0;

            var rootData = this._blueprint[rootId];

            foreach (var template in this._roomProviders[rootData].RandomOrderAccess(rng)) {
                evolved.Add(new PlacedRoom(template, Vector2.zero));

                // found a complete level
                if (this.EvolveNode(evolved, rng, rootId)) {
                    found = true;
                    break;
                }

                // if we could not evolve with the current template, loop and retry
                evolved.RemoveAt(rootId);
            }

            // level is not solvable
            if (!found) return false;

            this.ProcessPlacedRooms(evolved);

            return true;
        }

        private bool EvolveNode(List<PlacedRoom> evolved, Random rng, int lastId) {
            var placedRoom = evolved[lastId];
            return placedRoom.Template.doors.RandomOrderAccess(rng)
                .Any(door => this.EvolveNodeDoor(evolved, rng, lastId, door));
        }

        private bool EvolveNodeDoor(List<PlacedRoom> evolved, Random rng, int lastId, DoorData lastDoor) {
            var nextRoomId = lastId + 1;
            var lastPlacedRoom = evolved[lastId];

            // reached end of blueprint branch
            if (nextRoomId >= this._blueprint.Count) return true;

            var nextRoom = this._blueprint[nextRoomId];
            var nextTemplates = this._roomProviders[nextRoom];

            foreach (var template in nextTemplates.RandomOrderAccess(rng)) {
                foreach (var nextDoor in template.doors.RandomOrderAccess(rng)) {
                    // door size mismatch
                    if (nextDoor.size != lastDoor.size) continue;

                    // TODO: would be more efficient to guess the delta instead of trying both?
                    // try door delta positions
                    foreach (var delta in nextDoor.AdjacentDeltas()) {
                        var nextPos = lastPlacedRoom.Position + lastDoor.position + delta - nextDoor.position;
                        var nextPlacedRoom = new PlacedRoom(template, nextPos);

                        // check overlap
                        if (nextPlacedRoom.DoOverlap(lastPlacedRoom) ||
                            evolved.Any(pr => nextPlacedRoom.DoOverlap(pr))) {
                            continue;
                        }

                        evolved.Add(nextPlacedRoom);

                        if (this.EvolveNode(evolved, rng, nextRoomId)) {
                            return true;
                        }

                        evolved.RemoveAt(nextRoomId);
                    }
                }
            }

            return false;
        }

        private void ProcessPlacedRooms(List<PlacedRoom> placedRooms) {
            var bounds = new Bounds();

            foreach (var placedRoom in placedRooms) {
                var roomVolume = new Rect(placedRoom.Position, placedRoom.Template.size);
                bounds.Encapsulate(new Bounds(roomVolume.center, roomVolume.size));

                foreach (var enemySpawner in placedRoom.Template.enemySpawners) {
                    this._enemySpawners.Add(placedRoom.Position + enemySpawner);
                }

                foreach (var spawnPoint in placedRoom.Template.spawnPoints) {
                    this._spawnPoints.Add(placedRoom.Position + spawnPoint);
                }

                foreach (var exitPoint in placedRoom.Template.exitPoints) {
                    this._exitPoints.Add(placedRoom.Position + exitPoint);
                }
            }

            this._placedRooms = placedRooms;
            this.Bounds = bounds;
        }
    }
}
