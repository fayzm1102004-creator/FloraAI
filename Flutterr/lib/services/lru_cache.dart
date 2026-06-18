import 'dart:collection';

class LRUCache<K, V> {
  final int capacity;
  final LinkedHashMap<K, V> _cache = LinkedHashMap<K, V>();

  LRUCache(this.capacity);

  V? get(K key) {
    if (!_cache.containsKey(key)) {
      return null;
    }
    // Remove and re-insert to make it the most recently used
    final V value = _cache.remove(key)!;
    _cache[key] = value;
    print("⚡ RAM Cache Hit for key: $key");
    return value;
  }

  void put(K key, V value) {
    if (_cache.containsKey(key)) {
      _cache.remove(key);
    } else if (_cache.length >= capacity) {
      // Remove the first item (least recently used)
      final K firstKey = _cache.keys.first;
      _cache.remove(firstKey);
      print("🗑️ RAM Cache Evicted oldest item: $firstKey");
    }
    _cache[key] = value;
  }

  void remove(K key) {
    _cache.remove(key);
  }

  void clear() {
    _cache.clear();
  }

  int get length => _cache.length;
}
