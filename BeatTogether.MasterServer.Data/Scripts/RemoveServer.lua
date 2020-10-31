if redis.call("EXISTS", @serverKey) == 0 then
    return false
end
local hashEntries = redis.call("HGETALL", @serverKey)
redis.call("DEL", @serverKey);
redis.call("HDEL", @serversByHostUserIdKey, hashEntries["HostUserId"])
if tonumber(hashEntries["IsPublic"]) == 1 then
    redis.call("ZREM", @publicServersByPlayerCountKey, hashEntries["Code"])
else
    redis.call("ZREM", @privateServersByPlayerCountKey, hashEntries["Code"])
end
return true
