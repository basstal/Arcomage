# ruff: noqa: E501
from whiterice.tools.generate_config import GenerateConfig
import whiterice
from google.protobuf.json_format import MessageToJson
import os

def main():
    output_filename = GenerateConfig().run()
    config:whiterice.WhitericeConfig = whiterice.common.config.load_config(output_filename, whiterice.WhitericeConfig)
    config.required.unity_build_output = os.path.join(config.required.unity_project, 'Output')
    config.optional.yoo_asset.enabled = True
    config.optional.yoo_asset.default_package = 'DefaultPackage'
    config.optional.yoo_asset.incremental_build = True
    
    with open(output_filename, 'w') as f:
        f.write(MessageToJson(config))


main()