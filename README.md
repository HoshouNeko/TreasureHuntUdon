<div align="center">
  <p>Treasure Hunt Udon<br>
  persistance game for VRChat worlds</p>
  <a href="https://github.com/HoshouNeko/TreasureHuntUdon">
    <img alt="" height="400" src="./DOCS/TreasureHuntDemo.gif">
  </a>

</div>

## Installation
-  Download latest release: https://github.com/HoshouNeko/TreasureHuntUdon/releases
-  Import into your project
-  Install TextMeshPro (unity should ask you after import)

###  Setup:
- Drop "Treasure hunt" prefab into your scene
- Open "Locations". Duplicate zones, position and scale to designate areas where treasures will be spawned. You can disable or delete mesh renderer after positioning. You do not have to enable colliders.
Every time new treasure is spawned, system chooses random box, and random position inside it for spawn. You can increase spawn chances by making more boxes in same area.
- Drop your meshes into "Treasures". Duplicate and rename them so same items will have exactly the same name.
System will use names of props there to build persistance keys and menus. You can increase chances of specific item spawn by making more duplicates.
- Drop "Persistance Prize" into scene. In inspector write names of keys to check (treasure names), and how much points each treasure adds to score(treasure points). Set prize score cost. Drop prize into "Container".
When player finds enough treasures, "container" is enabled for everyone. You can put anything you want there: pickups, scripts, etc...
Once prize is unlocked, it will be enabled every time player joins the world. 

## Demo World

https://vrchat.com/home/world/wrld_6918bec7-0b2a-4541-bb3a-5638373bca7c

## License

[MIT](/LICENSE.md)

Credit and tip if you can~
#### [Ko-fi](https://ko-fi.com/hoshouneko) | [LinkTree](https://linktr.ee/hoshouneko) | [Discord](https://discord.gg/nXbGFqkQf8)
