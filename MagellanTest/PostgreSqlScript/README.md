Put the PostgreSQL script of Part 1 here.

CREATE OR REPLACE FUNCTION Get_Total_Cost(input_item_name VARCHAR(50))
RETURNS INTEGER AS $$
DECLARE
parent INTEGER := NULL;
total_cost INTEGER := 0;
child_cost INTEGER := 0;
self_id INTEGER := NULL;
BEGIN
SELECT item_id, item_cost, parent_item INTO self_id, total_cost, parent FROM Item WHERE item_name = input_item_name;

    IF parent IS NOT NULL THEN
    	RETURN NULL;
    END IF;

    WITH RECURSIVE child_items AS (
        SELECT item_id, item_name, parent_item, item_cost
        FROM Item
        WHERE parent_item = self_id
        UNION ALL
        SELECT i.item_id, i.item_name, i.parent_item, i.item_cost
        FROM Item i
        INNER JOIN child_items ci ON i.parent_item = ci.item_id
    )
    SELECT SUM(item_cost) INTO child_cost FROM child_items WHERE parent_item IS NOT NULL;

    RETURN total_cost + COALESCE(child_cost, 0);

END;

$$
LANGUAGE plpgsql;
$$
