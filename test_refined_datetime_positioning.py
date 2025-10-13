#!/usr/bin/env python3
"""
This is a demonstration of how the positioning bug was fixed with refined end time logic.

BEFORE: The graph positioning only considered DateOnly, which ignored time components
AFTER: The graph positioning considers the full DateTime, with smart end time calculation

The refined logic:
- If the last data point is on the same day as the end date, use that time as the end time
- Otherwise, use the beginning of the end date (TimeOnly.MinValue)
"""

from datetime import datetime, date, time

def old_positioning_logic(timestamp, start_date, end_date):
    """
    OLD: Uses only the date part, ignoring time
    """
    entry_date = timestamp.date()
    days_from_start = (entry_date - start_date).days
    total_days = (end_date - start_date).days
    return days_from_start / total_days if total_days > 0 else 0

def calculate_end_datetime(end_date, data_points):
    """
    NEW: Smart end time calculation based on actual data
    """
    if not data_points:
        return datetime.combine(end_date, time.min)
    
    # Find the last data point chronologically
    last_data_point = max(data_points, key=lambda dp: dp)
    last_data_point_date = last_data_point.date()
    
    # If the last data point is on the same day as the requested end date, 
    # use the time from that data point
    if last_data_point_date == end_date:
        return last_data_point
    
    # Otherwise, use the beginning of the end date
    return datetime.combine(end_date, time.min)

def new_positioning_logic(timestamp, start_datetime, end_datetime):
    """
    NEW: Uses full datetime including time components
    """
    time_from_start = timestamp - start_datetime
    total_time_span = end_datetime - start_datetime
    return time_from_start.total_seconds() / total_time_span.total_seconds() if total_time_span.total_seconds() > 0 else 0

print("REFINED END TIME LOGIC DEMONSTRATION:")
print("====================================")

# Scenario 1: Last data point is on the same day as end date
print("\nSCENARIO 1: Last data point on same day as end date")
print("---------------------------------------------------")
start_date = date(2024, 1, 1)
end_date = date(2024, 1, 1)  # Same day as data points

data_points_scenario1 = [
    datetime(2024, 1, 1, 9, 0),   # 9:00 AM
    datetime(2024, 1, 1, 17, 0)   # 5:00 PM (last point)
]

start_datetime = datetime.combine(start_date, time.min)
end_datetime_refined = calculate_end_datetime(end_date, data_points_scenario1)

print(f"Date range: {start_date} to {end_date}")
print(f"Data points: {[str(dp) for dp in data_points_scenario1]}")
print(f"Start datetime: {start_datetime}")
print(f"End datetime (refined): {end_datetime_refined} ← Uses last data point time!")

pos1 = new_positioning_logic(data_points_scenario1[0], start_datetime, end_datetime_refined)
pos2 = new_positioning_logic(data_points_scenario1[1], start_datetime, end_datetime_refined)
print(f"Position 1 (9:00 AM): {pos1:.6f}")
print(f"Position 2 (5:00 PM): {pos2:.6f}")

# Scenario 2: Last data point is NOT on the same day as end date
print("\nSCENARIO 2: Last data point NOT on same day as end date")
print("------------------------------------------------------")
start_date = date(2024, 1, 1)
end_date = date(2024, 1, 3)  # Different day from last data point

data_points_scenario2 = [
    datetime(2024, 1, 1, 9, 0),   # 9:00 AM
    datetime(2024, 1, 2, 17, 0)   # 5:00 PM next day (last point)
]

start_datetime = datetime.combine(start_date, time.min)
end_datetime_refined = calculate_end_datetime(end_date, data_points_scenario2)

print(f"Date range: {start_date} to {end_date}")
print(f"Data points: {[str(dp) for dp in data_points_scenario2]}")
print(f"Start datetime: {start_datetime}")
print(f"End datetime (refined): {end_datetime_refined} ← Uses beginning of end date!")

pos1 = new_positioning_logic(data_points_scenario2[0], start_datetime, end_datetime_refined)
pos2 = new_positioning_logic(data_points_scenario2[1], start_datetime, end_datetime_refined)
print(f"Position 1 (Jan 1, 9:00 AM): {pos1:.6f}")
print(f"Position 2 (Jan 2, 5:00 PM): {pos2:.6f}")

print("\nBENEFITS OF REFINED LOGIC:")
print("- More accurate positioning based on actual data extent")
print("- Prevents unnecessary empty space when data ends before end date")
print("- Maintains proportional spacing across the actual time range")