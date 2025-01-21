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
        public static void Shuffle<T>(this IList<T> ts, Random rng) {
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
            int[] values = Enumerable.Range(0, list.Count).ToArray();
            values.Shuffle(rng);

            foreach (var index in values) {
                yield return list[index];
            }
        }
    }

    public class PlacedRoom {
        public RoomData Template { get; private set; }
        public Vector2 Position { get; private set; }

        public PlacedRoom(RoomData template, Vector2 position) {
            Template = template;
            Position = position;
        }

        public bool DoOverlap(PlacedRoom lastPlacedRoom) {
            var selfRect = Template.size;
            var otherRect = lastPlacedRoom.Template.size;

            return Position.x < lastPlacedRoom.Position.x + otherRect.x
                   && Position.x + selfRect.x > lastPlacedRoom.Position.x
                   && Position.y < lastPlacedRoom.Position.y + otherRect.y
                   && Position.y + selfRect.y > lastPlacedRoom.Position.y;
        }
    }

    public class LevelEvolver {
        private readonly List<RoomKind> _blueprint;
        private readonly Dictionary<RoomKind, List<RoomData>> _roomProviders;

        private List<PlacedRoom> _placedRooms = new();
        private List<Vector2> _enemySpawners = new();
        private List<Vector2> _spawnPoints = new();

        public ReadOnlyCollection<PlacedRoom> PlacedRooms => _placedRooms.AsReadOnly();
        public ReadOnlyCollection<Vector2> EnemySpawners => _enemySpawners.AsReadOnly();
        public ReadOnlyCollection<Vector2> SpawnPoints => _spawnPoints.AsReadOnly();

        public LevelEvolver(List<RoomKind> blueprint, Dictionary<RoomKind, List<RoomData>> roomProviders) {
            _blueprint = blueprint;
            _roomProviders = roomProviders;
        }

        public bool EvolveRoot(Random rng) {
            var found = false;

            var evolved = new List<PlacedRoom>();
            const int rootId = 0;

            var rootData = _blueprint[rootId];

            foreach (var template in _roomProviders[rootData].RandomOrderAccess(rng)) {
                evolved.Add(new PlacedRoom(template, Vector2.zero));

                // found a complete level
                if (EvolveNode(ref evolved, rng, rootId)) {
                    found = true;
                    break;
                }

                // if we could not evolve with the current template, loop and retry
                evolved.RemoveAt(rootId);
            }

            // level is not solvable
            if (!found) return false;

            ProcessPlacedRooms(evolved);

            return true;
        }

        private bool EvolveNode(ref List<PlacedRoom> evolved, Random rng, int lastId) {
            var placedRoom = evolved[lastId];

            foreach (var door in placedRoom.Template.doors.RandomOrderAccess(rng)) {
                if (EvolveNodeDoor(ref evolved, rng, lastId, door)) return true;
            }

            return false;
        }

        private bool EvolveNodeDoor(ref List<PlacedRoom> evolved, Random rng, int lastId, DoorData lastDoor) {
            var nextRoomId = lastId + 1;
            var lastPlacedRoom = evolved[lastId];

            // reached end of blueprint branch
            if (nextRoomId >= _blueprint.Count) return true;

            var nextRoom = _blueprint[nextRoomId];
            var nextTemplates = _roomProviders[nextRoom];

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

                        if (EvolveNode(ref evolved, rng, nextRoomId)) {
                            return true;
                        }

                        evolved.RemoveAt(nextRoomId);
                    }
                }
            }

            return false;
        }

        private void ProcessPlacedRooms(List<PlacedRoom> placedRooms) {
            foreach (var placedRoom in placedRooms) {
                foreach (var enemySpawner in placedRoom.Template.enemySpawners) {
                    _enemySpawners.Add(placedRoom.Position + enemySpawner);
                }

                foreach (var spawnPoint in placedRoom.Template.spawnPoints) {
                    _spawnPoints.Add(placedRoom.Position + spawnPoint);
                }
            }

            _placedRooms = placedRooms;
        }
    }
}
