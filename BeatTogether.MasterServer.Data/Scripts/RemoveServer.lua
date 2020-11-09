if redis.call("EXISTS", @serverKey) == 0 then
    return false
end
local hashEntries = redis.call("HGETALL", @serverKey)
redis.call("DEL", @serverKey);
redis.call("HDEL", @serversByHostUserIdKey, tostring(hashEntries["HostUserId"]))
if tonumber(hashEntries["IsPublic"]) == 1 then
    redis.call("ZREM", @publicServersByPlayerCountKey, tostring(hashEntries["Code"]))
else
    redis.call("ZREM", @privateServersByPlayerCountKey, tostring(hashEntries["Code"]))
end
return true
