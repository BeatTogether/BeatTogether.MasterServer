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
redis.call("HDEL", @serversByHostUserIdKey, hashEntryTable["HostUserId"])
redis.call("HDEL", @serversByCodeKey, hashEntryTable["Code"])
if tonumber(hashEntryTable["IsPublic"]) == 1 then
    redis.call("ZREM", @publicServersByPlayerCountKey, @secret)
else
    redis.call("ZREM", @privateServersByPlayerCountKey, @secret)
end
return true
