"""Generate CC0-style placeholder GLB models for Phase 10.5 (no external deps)."""

from __future__ import annotations

import json
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "Assets" / "Models" / "Placeholder" / "Glb"


def tri(a, b, c):
    return [a, b, c]


def box(w, h, d, ox=0.0, oy=0.0, oz=0.0):
    x0, x1 = ox - w * 0.5, ox + w * 0.5
    y0, y1 = oy, oy + h
    z0, z1 = oz - d * 0.5, oz + d * 0.5
    corners = [
        (x0, y0, z0),
        (x1, y0, z0),
        (x1, y0, z1),
        (x0, y0, z1),
        (x0, y1, z0),
        (x1, y1, z0),
        (x1, y1, z1),
        (x0, y1, z1),
    ]
    faces = [
        tri(0, 1, 2),
        tri(0, 2, 3),
        tri(4, 6, 5),
        tri(4, 7, 6),
        tri(0, 4, 5),
        tri(0, 5, 1),
        tri(1, 5, 6),
        tri(1, 6, 2),
        tri(2, 6, 7),
        tri(2, 7, 3),
        tri(3, 7, 4),
        tri(3, 4, 0),
    ]
    verts = []
    for face in faces:
        for idx in face:
            verts.extend(corners[idx])
    return verts


def cone(radius, height, segments, oy=0.0, ox=0.0, oz=0.0):
    verts = []
    apex = (ox, oy + height, oz)
    for i in range(segments):
        a0 = i * 2.0 * 3.14159265 / segments
        a1 = (i + 1) * 2.0 * 3.14159265 / segments
        p0 = (ox + radius * __import__("math").cos(a0), oy, oz + radius * __import__("math").sin(a0))
        p1 = (ox + radius * __import__("math").cos(a1), oy, oz + radius * __import__("math").sin(a1))
        verts.extend([*apex, *p0, *p1])
        verts.extend([0.0, oy, 0.0, *p0, *p1])
    return verts


def write_glb(path: Path, vertices: list[float], name: str) -> None:
    import math

    byte_length = len(vertices) * 4
    bin_blob = struct.pack(f"<{len(vertices)}f", *vertices)
    pad = (4 - len(bin_blob) % 4) % 4
    bin_blob += b"\x00" * pad

    min_vals = [min(vertices[i::3]) for i in range(3)]
    max_vals = [max(vertices[i::3]) for i in range(3)]

    gltf = {
        "asset": {"version": "2.0", "generator": "aoe-placeholder-glb"},
        "scene": 0,
        "scenes": [{"nodes": [0]}],
        "nodes": [{"mesh": 0, "name": name}],
        "meshes": [
            {
                "primitives": [
                    {
                        "attributes": {"POSITION": 0},
                        "mode": 4,
                    }
                ]
            }
        ],
        "accessors": [
            {
                "bufferView": 0,
                "componentType": 5126,
                "count": len(vertices) // 3,
                "type": "VEC3",
                "min": min_vals,
                "max": max_vals,
            }
        ],
        "bufferViews": [{"buffer": 0, "byteOffset": 0, "byteLength": byte_length}],
        "buffers": [{"byteLength": byte_length}],
    }

    json_bytes = json.dumps(gltf, separators=(",", ":")).encode("utf-8")
    json_pad = (4 - len(json_bytes) % 4) % 4
    json_bytes += b" " * json_pad

    total_length = 12 + 8 + len(json_bytes) + 8 + len(bin_blob)
    header = struct.pack("<4sII", b"glTF", 2, total_length)
    json_chunk = struct.pack("<I4s", len(json_bytes), b"JSON") + json_bytes
    bin_chunk = struct.pack("<I4s", len(bin_blob), b"BIN\x00") + bin_blob
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(header + json_chunk + bin_chunk)


def build_models() -> dict[str, list[float]]:
    import math

    villager = []
    villager.extend(box(0.55, 1.0, 0.35, oy=0.2))
    villager.extend(box(0.32, 0.32, 0.32, oy=1.25))

    militia = []
    militia.extend(box(0.6, 1.05, 0.4, oy=0.15))
    militia.extend(box(0.34, 0.34, 0.34, oy=1.28))
    militia.extend(box(0.08, 0.7, 0.08, ox=0.42, oy=0.45))

    town_center = []
    town_center.extend(box(7.5, 3.5, 7.5, oy=0.0))
    town_center.extend(box(2.0, 2.5, 2.0, oy=3.5))

    house = []
    house.extend(box(3.8, 2.5, 3.8, oy=0.0))
    for i in range(8):
        a0 = i * 2.0 * math.pi / 8
        a1 = (i + 1) * 2.0 * math.pi / 8
        r = 2.8
        y_base = 2.5
        p0 = (r * math.cos(a0), y_base, r * math.sin(a0))
        p1 = (r * math.cos(a1), y_base, r * math.sin(a1))
        apex = (0.0, 4.2, 0.0)
        house.extend([*apex, *p0, *p1])

    barracks = []
    barracks.extend(box(5.8, 3.2, 5.8, oy=0.0))
    barracks.extend(box(1.2, 0.8, 4.0, oy=3.2, oz=2.2))

    tree = []
    tree.extend(box(0.35, 1.2, 0.35, oy=0.0))
    tree.extend(cone(1.4, 2.8, 10, oy=1.0))

    return {
        "villager.glb": villager,
        "militia.glb": militia,
        "town_center.glb": town_center,
        "house.glb": house,
        "barracks.glb": barracks,
        "tree.glb": tree,
    }


def main() -> None:
    for filename, vertices in build_models().items():
        path = OUT_DIR / filename
        write_glb(path, vertices, filename.replace(".glb", ""))
        print(f"Wrote {path}")


if __name__ == "__main__":
    main()
