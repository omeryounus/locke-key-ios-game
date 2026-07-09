import os

base_dir = "/Users/omer/.gemini/antigravity/scratch/locke-key-ios-game/Assets/GrokWireframes"

mappings = {
    # Keys
    "grok-7093cb57-b076-46fd-8294-0edf7db16b0e.jpg": "key_anywhere.jpg",
    "grok-28a0a987-f08b-436d-bab0-aae7aa6928fb.jpg": "key_anywhere_v1.jpg",
    "grok-48a906e7-3fab-4b4d-998d-9f74685376f8.jpg": "key_head.jpg",
    "grok-38e37823-4485-4d4e-89a5-79d60943d844.jpg": "key_mending.jpg",
    "grok-3e4c8428-61c5-4a34-913e-d023c74a41f5.jpg": "key_omega.jpg",
    "grok-0949ad52-d51e-4249-aee6-e877b1b9db1e.jpg": "key_omega_v1.jpg",
    "grok-55c97fe3-859d-4a3d-9017-23edfdca06d1.jpg": "key_ghost.jpg",
    "grok-ac0ea759-90e4-4e71-b038-5f9783ea1c13.jpg": "key_ghost_v1.jpg",
    "grok-158210d3-f2f1-41dc-94cf-b7cc4f14609e.jpg": "key_shadow.jpg",
    "grok-adc15a52-a188-48c2-bea3-693d7e4bd771.jpg": "key_shadow_v1.jpg",
    "grok-60cf46bf-be11-46dc-b393-78c64dde101b.jpg": "key_echo.jpg",
    "grok-782c19b8-3315-4487-82a4-0149f94efab2.jpg": "key_echo_v1.jpg",
    "grok-6b15ac88-331d-4b39-96c3-929562e93136.jpg": "key_matchstick.jpg",
    "grok-260920e8-ae3b-4d4b-8c91-5d6c94abc701.jpg": "key_matchstick_v1.jpg",
    "grok-1b2c199d-5292-4d4e-9f05-373925ccacd1.jpg": "key_mirror.jpg",
    "grok-41fc7be7-7547-4d7e-bec8-2ca0df77400a.jpg": "key_mirror_v1.jpg",
    "grok-eec5f853-ccc6-41bd-ad17-7605a6ab84e4.jpg": "key_music_box.jpg",
    "grok-59db093f-0836-406e-b467-ae8e9ac82ee0.jpg": "key_music_box_v1.jpg",
    "grok-d67c5ab2-f17e-4d78-aea9-c245d8773169.jpg": "key_animal.jpg",
    "grok-a7b8d0c5-f4b2-4f02-8684-84047df71dd2.jpg": "key_animal_v1.jpg",
    "grok-700b886f-e7f8-4aa3-b295-1ea96f2334c6.jpg": "key_identity.jpg",
    "grok-0c1ba306-5edb-4403-a678-1c92547886d6.jpg": "key_identity_v1.jpg",
    "grok-c1ecaabb-7fac-4dc5-b72f-06de31f14da7.jpg": "key_alpha.jpg",
    "grok-4698a8cb-0519-40ad-b74d-cdb8bb90fad5.jpg": "key_alpha_v1.jpg",

    # Backgrounds
    "grok-5c51f6bb-e890-4b87-b090-055aa3ea9e5b.jpg": "bg_keyhouse_foyer_16x9.jpg",
    "grok-326a1ae3-6a5c-47ae-8f77-d7aa5cc6930b.jpg": "bg_keyhouse_foyer_16x9_v1.jpg",
    "grok-326a1ae3-6a5c-47ae-8f77-d7aa5cc6930b (1).jpg": "bg_keyhouse_foyer_16x9_v1_dup.jpg",
    "grok-74ffc370-ccd6-40a1-9e57-6c9ff9de051e.jpg": "bg_keyhouse_foyer_9x16.jpg",
    "grok-8c0a98ae-b9ec-4256-9bad-0c5bdf3314cc.jpg": "bg_wellhouse_exterior.jpg",
    "grok-d983fb1d-f0bd-4fcd-a65f-1e3abad8080d.jpg": "bg_wellhouse_exterior_v1.jpg",
    "grok-56f70726-d229-408b-b9f3-e27e01628c48.jpg": "bg_black_door_chamber.jpg",
    "grok-9049e971-07e7-4102-9244-793f4b62005e.jpg": "bg_black_door_chamber_v1.jpg",

    # Storyboard
    "grok-85b5a431-a52a-4777-acab-b578a6017c9e.jpg": "story_01_arrival.jpg",
    "grok-059afef4-fb13-4241-9e04-f556e87bb0c1.jpg": "story_02_first_discovery.jpg",
    "grok-c6e7114f-79c0-4cc9-acdc-8490696b15fd.jpg": "story_03_wellhouse_echo.jpg",
    "grok-7780eeb5-5bbe-495e-8a22-1eea341f497d.jpg": "story_04_black_door.jpg",

    # UI
    "grok-f301bca6-a3f8-4baf-afdb-e88e2247b7d6.jpg": "ui_key_slot_empty.jpg",
    "grok-21c65d75-75ad-4fef-a22c-a8eef4c1c756.jpg": "ui_key_slot_empty_v1.jpg",
    "grok-7426df8f-1f82-4d85-b726-8bf05948f5e5.jpg": "ui_btn_primary.jpg",
    "grok-9fcdd30e-4973-427c-b01e-5b4f3ac21fe6.jpg": "ui_codex_panel.jpg",

    # Wireframes
    "grok-b7dd23a9-8f93-4950-bc0e-7d2ee3775a53.jpg": "wireframe_main_scene_keyhouse.jpg",
    "grok-db1d7260-1122-460c-b45c-f81221455838.jpg": "wireframe_key_discovery.jpg",
    "grok-193f0ac5-ce83-4e1e-8aad-1eb3af769b5f.jpg": "wireframe_story_strip.jpg",
    "grok-fdb2e975-ba9d-4d95-9c2f-840360a01d92.jpg": "wireframe_key_ring.jpg",
    "grok-4c400301-9f3d-43ed-a1d4-76c30d51f3b3.jpg": "wireframe_foyer_detailed.jpg",
    "grok-bff9e55e-149a-492c-ab04-88b4dd1f584d.jpg": "wireframe_lock_3state.jpg",
    "grok-78da98d6-ce9d-44d5-8557-68a44cbe48b1.jpg": "wireframe_flowchart.jpg",
    "grok-4781a13c-56d6-485f-a754-6ca1455db837.jpg": "wireframe_chapter_map.jpg",
    "grok-4dacf6b8-a770-44c7-9f7d-b26ccc4e6e49.jpg": "wireframe_codex_discovery.jpg"
}

renamed_count = 0
not_found_count = 0

for uuid_name, clean_name in mappings.items():
    src_path = os.path.join(base_dir, uuid_name)
    dest_path = os.path.join(base_dir, clean_name)
    
    if os.path.exists(src_path):
        os.rename(src_path, dest_path)
        print(f"Renamed: {uuid_name} -> {clean_name}")
        renamed_count += 1
    else:
        print(f"Warning: {uuid_name} not found in Assets/GrokWireframes/")
        not_found_count += 1

print(f"\nDone! Renamed {renamed_count} files. {not_found_count} warnings.")
