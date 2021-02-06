#!/usr/bin/python

import utility as u
import re
import time,os

def get_bin():    
    return u.get_bin('protoc')

def get_protofiles(input_path):
    proto_files = []
    for path in input_path:
        print("proto path : " + path)
        proto_files += [f for f in u.get_files(path, ['proto'], recursive=True)]

    return proto_files


def make_args(input_path, search_path, purpose): 
    proto_files = get_protofiles(input_path)

    # tag = u.gen_tag('protobuf_' + purpose + str(u.string_hash(''.join(proto_files))))

    print(proto_files)
    if len(proto_files) > 0:
        additional_path = u.get_temp_path("cmd_args")
        u.write(additional_path, "\n".join(proto_files))
        args = ''        
        args = args + '--strip_source_info '
        args = args + '--ignore_options=urls:view:comment:fc:ec:evc '
        if u.is_ci_mode():
            args = args + '--ignore_options=NOAH.Proto.enum_tooltip:NOAH.Proto.field_tooltip '
        args = args + ' '.join(['--proto_path=' + p for p in search_path if u.exists(p)]) + ' @' + additional_path
        print ("args : " + args)
        return args
    else:
        return None

def generate_descriptor(input_path, pb_out, search_path):
    args = make_args(input_path, search_path, 'descriptor') 
    if args is not None:
        u.execute(get_bin(), '--include_source_info', '-o ' + pb_out, args)
        # u.touch(tag)


def generate_python(input_path, python_out, search_path):
    args = make_args(input_path, search_path, 'python')
    if args is not None:
        u.clear_dir(python_out)
        u.execute(get_bin(), '--python_out=' + python_out, args)
        u.touch(u.join_path(python_out, '__init__.py'))

        # HACK: Temporary fix for windows since it would cause error if a proto file is too large
        for file in u.get_files(python_out, ['py']):
            content = u.read(file)
            content = content.replace('serialized_options=None', 'serialized_options=\'\'')
            u.write(file, content)

        # u.touch(tag)


def generate_csharp(input_path, csharp_out, search_path):
    args = make_args(input_path, search_path, 'csharp')    
    if args is not None:
        u.del_files(u.get_files(csharp_out, ['cs']))        
        u.execute(get_bin(), '--csharp_out=' + csharp_out, args)

        for file in u.get_files(csharp_out, ['cs']):
            u.normalize_eol(file)

        # u.touch(tag)


def generate_go(input_path, go_out, search_path):
    args = make_args(input_path, search_path, 'go')   
    if args is not None:
        u.mkdir(go_out)
        u.del_files(u.get_files(go_out, ['go']))
        u.execute(get_bin(),
                    '--go_out=' + go_out,
                    '-I ' + input_path[0],
                    input_path[0] + '/*.proto')

        # u.touch(tag)
                    

def copy_proto_to_temp(input_path,dest_path):
    for path in input_path:
        dirs = u.get_dirs(path)
        for d in dirs:
            if 'google' in d:
                u.copytree(d,"{}/{}".format(dest_path,u.base_name_no_ext(d)))

        files = [f for f in u.get_files(path, ['proto'], recursive=True) if 'google' not in f]
        for f in files:
            u.copy(f,"{}/{}".format(dest_path,u.base_name(f)))    


def do_compile():
    # for noah
    # noah_input_path = u.get_res('NOAH/Modules/Proto/Editor/Proto')
    # noah_csharp_out = u.get_res('NOAH/Modules/Proto/Runtime/Scripts/Generated/Classes')
    # noah_pb_out = u.get_res('NOAH/Modules/Proto/Runtime/AssetBundle/NOAH/Data/pbdef.bytes')
    # for project
    # proj_input_path1 = u.get_res('NOAHExtension/Proto/Editor/Proto/Client')
    # proj_input_path2 = u.get_res('NOAHExtension/Proto/Editor/Proto/Common')
    # proj_input_path3 = u.get_res('NOAHExtension/Proto/Editor/Proto/ActService')
    proj_csharp_out = u.get_res('Scripts/ProtoGenerated/')
    if not u.exists(proj_csharp_out):
        u.mkdir(proj_csharp_out)
    # proj_pb_out = u.get_res('AssetBundle/Data/pbdef.bytes')
    # for python
    # python_out = u.get_script('res/pb')

    # noah_pb_path = u.get_temp_path("noah_proto_path")
    proj_pb_path = u.get_res("Proto/")
    # u.clear_dir(noah_pb_path)
    # u.clear_dir(proj_pb_path)

    # copy_proto_to_temp([noah_input_path], noah_pb_path)
    # copy_proto_to_temp([proj_input_path1, proj_input_path2, proj_input_path3],proj_pb_path)

    # for noah
    # generate_descriptor([noah_pb_path], noah_pb_out, [noah_pb_path])
    # generate_csharp([noah_pb_path], noah_csharp_out, [noah_pb_path])
    
    # for project
    # generate_descriptor([proj_pb_path], proj_pb_out, [proj_pb_path, noah_pb_path])
    generate_csharp([proj_pb_path], proj_csharp_out, [proj_pb_path])

    # for python
    # generate_python([noah_pb_path, proj_pb_path], python_out, [proj_pb_path, noah_pb_path])

    # u.clear_dir(noah_pb_path)
    # u.clear_dir(proj_pb_path)

def main():
    do_compile()


if __name__ == '__main__':
    main()
