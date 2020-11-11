if redis.call("EXISTS", @serverKey) == 0 then
    return false
end
local hashEntries = redis.call('HGETALL', @serverKey)
local hashEntryTable = {}
local nextKey
for i, j in ipairs(hashEntries) do
    if i % 2 == 1 then
		nextKey = j
	else
		hashEntryTable[nextKey] = j
	end
end
redis.call("DEL", @serverKey);
redis.call("HDEL", @serversByCodeKey, hashEntryTable["Code"])
redis.call("ZREM", @publicServersByPlayerCountKey, @secret)
return true
